package auth

import (
	"bytes"
	"encoding/json"
	"errors"
	"fmt"
	"net/http"
	"strings"
	"time"
)

var ErrInvalidAPIKey = errors.New("invalid api key")

type ControlAPIValidator struct {
	baseURL        string
	internalSecret string
	httpClient     *http.Client
}

func NewControlAPIValidator(baseURL string, internalSecret string) *ControlAPIValidator {
	return &ControlAPIValidator{
		baseURL:        strings.TrimRight(baseURL, "/"),
		internalSecret: internalSecret,
		httpClient: &http.Client{
			Timeout: 5 * time.Second,
		},
	}
}

func (v *ControlAPIValidator) Validate(apiKey string) (*ValidationResult, error) {
	body, err := json.Marshal(map[string]string{
		"apiKey": apiKey,
	})
	if err != nil {
		return nil, err
	}

	req, err := http.NewRequest(
		http.MethodPost,
		fmt.Sprintf("%s/internal/api-keys/validate", v.baseURL),
		bytes.NewReader(body),
	)
	if err != nil {
		return nil, err
	}

	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("X-Internal-Secret", v.internalSecret)

	res, err := v.httpClient.Do(req)
	if err != nil {
		return nil, err
	}
	defer res.Body.Close()

	if res.StatusCode != http.StatusOK {
		return nil, ErrInvalidAPIKey
	}

	var result ValidationResult
	if err := json.NewDecoder(res.Body).Decode(&result); err != nil {
		return nil, err
	}

	if !result.Valid {
		return nil, ErrInvalidAPIKey
	}

	return &result, nil
}
