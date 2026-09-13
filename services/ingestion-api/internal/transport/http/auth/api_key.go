package auth

import (
	"errors"
	"net/http"
	"strings"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/constants"
)

var (
	ErrMissingAuthorization = errors.New("missing authorization header")
	ErrInvalidAuthorization = errors.New("invalid authorization header")
)

func ExtractAPIKey(r *http.Request) (string, error) {
	header := strings.TrimSpace(r.Header.Get(constants.AuthorizationHeader))
	if header == "" {
		return "", ErrMissingAuthorization
	}

	scheme, value, ok := strings.Cut(header, " ")
	if !ok {
		return "", ErrInvalidAuthorization
	}

	if !strings.EqualFold(scheme, constants.APIKeyScheme) {
		return "", ErrInvalidAuthorization
	}

	apiKey := strings.TrimSpace(value)
	if apiKey == "" {
		return "", ErrInvalidAuthorization
	}
	return apiKey, nil
}
