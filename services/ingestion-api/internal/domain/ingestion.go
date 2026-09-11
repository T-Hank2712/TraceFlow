package domain

import "context"

type LogRequest struct {
	Service string `json:"service"`
	Level   string `json:"level"`
	Message string `json:"message"`
}

type LogEvent struct {
	Service string `json:"service"`
	Level   string `json:"level"`
	Message string `json:"message"`
}

type LogPublisher interface {
	Publish(ctx context.Context, value []byte) error
}

type IngestionService interface {
	AcceptLog(ctx context.Context, apiKey string, request LogRequest) (*APIKeyMetadata, error)
}
