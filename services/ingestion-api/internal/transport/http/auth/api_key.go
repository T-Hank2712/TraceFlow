package auth

import (
	"errors"
	"net/http"
	"strings"
)

var (
	ErrMissingAuthorization = errors.New("missing authorization header")
	ErrInvalidAuthorization = errors.New("invalid authorization header")
)

func ExtractAPIKey(r *http.Request) (string, error) {
	header := strings.TrimSpace(r.Header.Get("Authorization"))
	if header == "" {
		return "", ErrMissingAuthorization
	}

	scheme, value, ok := strings.Cut(header, " ")
	if !ok {
		return "", ErrInvalidAuthorization
	}

	if !strings.EqualFold(scheme, "ApiKey") {
		return "", ErrInvalidAuthorization
	}

	apiKey := strings.TrimSpace(value)
	if apiKey == "" {
		return "", ErrInvalidAuthorization
	}
	return apiKey, nil
}
