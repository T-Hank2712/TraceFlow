package repository

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"net/http"

	"github.com/T-Hank2712/traceflow/log-processor/internal/model"
	"github.com/opensearch-project/opensearch-go/v4"
	"github.com/opensearch-project/opensearch-go/v4/opensearchapi"
)

type OpenSearchRepository struct {
	client *opensearch.Client
	index  string
}

func NewOpenSearchRepository(
	url string,
	username string,
	password string,
	index string,
) (*OpenSearchRepository, error) {
	client, err := opensearch.NewClient(opensearch.Config{
		Addresses: []string{url},
		Username:  username,
		Password:  password,
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
