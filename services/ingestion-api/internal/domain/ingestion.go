package domain

import (
	"context"
	"errors"
)

type LogRequest struct {
	Timestamp     string         `json:"timestamp,omitempty"`
	Service       string         `json:"service"`
	Level         string         `json:"level"`
	Message       string         `json:"message"`
	TraceID       string         `json:"traceId,omitempty"`
	CorrelationID string         `json:"correlationId,omitempty"`
	Metadata      map[string]any `json:"metadata,omitempty"`
}

type LogEvent struct {
	EventID       string         `json:"eventId"`
	Timestamp     string         `json:"timestamp"`
	ReceivedAt    string         `json:"receivedAt"`
	WorkspaceID   string         `json:"workspaceId"`
	ProjectID     string         `json:"projectId"`
	ApplicationID string         `json:"applicationId"`
	Environment   string         `json:"environment"`
	Service       string         `json:"service"`
	Level         string         `json:"level"`
	Message       string         `json:"message"`
	TraceID       string         `json:"traceId,omitempty"`
	CorrelationID string         `json:"correlationId,omitempty"`
	Metadata      map[string]any `json:"metadata,omitempty"`
}

type LogPublisher interface {
	Publish(ctx context.Context, value []byte) error
}

type IngestionService interface {
	AcceptLog(ctx context.Context, apiKey string, request LogRequest) (*APIKeyMetadata, error)
}

var ErrInvalidLogPayload = errors.New("invalid log payload")
