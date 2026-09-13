package repository

import (
	"bytes"
	"context"
	"crypto/tls"
	"encoding/json"
	"fmt"
	"io"
	"net/http"

	"github.com/T-Hank2712/traceflow/log-processor/internal/model"
	"github.com/opensearch-project/opensearch-go/v4"
	"github.com/opensearch-project/opensearch-go/v4/opensearchapi"
)

type OpenSearchRepository struct {
	client *opensearch.Client
	index  string
}

type BulkIndexResult struct {
	FailedItems []BulkIndexFailure
}

type BulkIndexFailure struct {
	Index  int
	Reason string
}

type bulkIndexResponse struct {
	Errors bool            `json:"errors"`
	Items  []bulkIndexItem `json:"items"`
}

type bulkIndexItem struct {
	Index bulkIndexItemResult `json:"index"`
}

type bulkIndexItemResult struct {
	Status int                 `json:"status"`
	Error  *bulkIndexItemError `json:"error,omitempty"`
}

type bulkIndexItemError struct {
	Type   string `json:"type"`
	Reason string `json:"reason"`
}

func (r BulkIndexResult) HasFailures() bool {
	return len(r.FailedItems) > 0
}

func NewOpenSearchRepository(
	url string,
	username string,
	password string,
	index string,
	skipTLSVerify bool,
) (*OpenSearchRepository, error) {
	client, err := opensearch.NewClient(opensearch.Config{
		Addresses: []string{url},
		Username:  username,
		Password:  password,
		Transport: &http.Transport{
			TLSClientConfig: &tls.Config{
				InsecureSkipVerify: skipTLSVerify,
			},
		},
	})

	if err != nil {
		return nil, err
	}

	return &OpenSearchRepository{
		client: client,
		index:  index,
	}, nil
}

func (r *OpenSearchRepository) IndexLog(
	ctx context.Context,
	event *model.LogEvent,
) error {
	body, err := json.Marshal(event)
	if err != nil {
		return fmt.Errorf("failed to marshal log event: %w", err)
	}

	req := opensearchapi.IndexReq{
		Index: r.index,
		Body:  bytes.NewReader(body),
	}

	res, err := opensearch.Do(
		ctx,
		r.client,
		http.MethodPost,
		req,
		(*opensearchapi.IndexResp)(nil),
	)

	if err != nil {
		return fmt.Errorf("failed to index log event: %w", err)
	}

	defer res.Body.Close()

	if res.IsError() {
		return fmt.Errorf(
			"OpenSearch returned error: status=%s",
			res.Status(),
		)
	}

	return nil
}

func (r *OpenSearchRepository) IndexLogs(
	ctx context.Context,
	events []model.LogEvent,
) (*BulkIndexResult, error) {
	if len(events) == 0 {
		return &BulkIndexResult{}, nil
	}

	body, err := buildBulkIndexBody(r.index, events)
	if err != nil {
		return nil, err
	}

	req := opensearchapi.BulkReq{
		Body: bytes.NewReader(body),
	}

	res, err := opensearch.Do(
		ctx,
		r.client,
		http.MethodPost,
		req,
		(*opensearchapi.BulkResp)(nil),
	)

	if err != nil {
		return nil, fmt.Errorf("failed to bulk index log events: %w", err)
	}

	defer res.Body.Close()

	if res.IsError() {
		return nil, fmt.Errorf(
			"OpenSearch bulk returned error: status=%s",
			res.Status(),
		)
	}

	return parseBulkIndexResponse(res.Body)
}

func buildBulkIndexBody(
	index string,
	events []model.LogEvent,
) ([]byte, error) {
	var buffer bytes.Buffer

	for _, event := range events {
		action := map[string]map[string]string{
			"index": {
				"_index": index,
			},
		}

		actionLine, err := json.Marshal(action)
		if err != nil {
			return nil, fmt.Errorf("failed to marshal bulk action: %w", err)
		}

		documentLine, err := json.Marshal(event)
		if err != nil {
			return nil, fmt.Errorf("failed to marshal bulk document: %w", err)
		}

		buffer.Write(actionLine)
		buffer.WriteByte('\n')
		buffer.Write(documentLine)
		buffer.WriteByte('\n')
	}

	return buffer.Bytes(), nil
}

func parseBulkIndexResponse(body io.Reader) (*BulkIndexResult, error) {
	var response bulkIndexResponse

	if err := json.NewDecoder(body).Decode(&response); err != nil {
		return nil, fmt.Errorf("failed to decode bulk index response: %w", err)
	}

	result := &BulkIndexResult{}

	if !response.Errors {
		return result, nil
	}

	for index, item := range response.Items {
		if item.Index.Status >= 200 && item.Index.Status < 300 {
			continue
		}

		reason := fmt.Sprintf("OpenSearch bulk item failed with status %d", item.Index.Status)

		if item.Index.Error != nil {
			reason = fmt.Sprintf(
				"%s: %s",
				item.Index.Error.Type,
				item.Index.Error.Reason,
			)
		}

		result.FailedItems = append(
			result.FailedItems,
			BulkIndexFailure{
				Index:  index,
				Reason: reason,
			},
		)
	}

	return result, nil
}
