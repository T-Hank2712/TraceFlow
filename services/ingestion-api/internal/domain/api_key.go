package domain

import (
	"context"
	"errors"
)

var ErrInvalidAPIKey = errors.New("invalid api key")

type APIKeyMetadata struct {
	WorkspaceID   string
	ProjectID     string
	ApplicationID string
	Environment   string
}

type APIKeyValidator interface {
	Validate(ctx context.Context, apiKey string) (*APIKeyMetadata, error)
}
