package domain

import "errors"

var ErrInvalidAPIKey = errors.New("invalid api key")

type APIKeyMetadata struct {
	WorkspaceID   string
	ProjectID     string
	ApplicationID string
	Environment   string
}
