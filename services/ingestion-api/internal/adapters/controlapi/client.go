package controlapi

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"strings"
	"time"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/constants"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/domain"
)

type Client struct {
	baseURL        string
	internalSecret string
	httpClient     *http.Client
}

func NewClient(
	baseURL string,
	internalSecret string,
	timeout time.Duration,
) *Client {
	return &Client{
		baseURL:        strings.TrimRight(baseURL, "/"),
		internalSecret: internalSecret,
		httpClient: &http.Client{
			Timeout: timeout,
		},
	}
}

func (c *Client) Validate(ctx context.Context, apiKey string) (*domain.APIKeyMetadata, error) {
	body, err := json.Marshal(map[string]string{
		"apiKey": apiKey,
	})
	if err != nil {
		return nil, err
	}

	req, err := http.NewRequestWithContext(
		ctx,
		http.MethodPost,
		fmt.Sprintf("%s/internal/api-keys/validate", c.baseURL),
		bytes.NewReader(body),
	)
	if err != nil {
		return nil, err
	}

	req.Header.Set("Content-Type", "application/json")
	req.Header.Set(constants.InternalSecretHeader, c.internalSecret)

	res, err := c.httpClient.Do(req)
	if err != nil {
		return nil, err
	}
	defer res.Body.Close()

	if res.StatusCode != http.StatusOK {
		return nil, domain.ErrInvalidAPIKey
	}

	var result validateAPIKeyResponse
	if err := json.NewDecoder(res.Body).Decode(&result); err != nil {
		return nil, err
	}

	if !result.Valid {
		return nil, domain.ErrInvalidAPIKey
	}

	return &domain.APIKeyMetadata{
		WorkspaceID:   result.WorkspaceID,
		ProjectID:     result.ProjectID,
		ApplicationID: result.ApplicationID,
		Environment:   result.Environment,
	}, nil
}
