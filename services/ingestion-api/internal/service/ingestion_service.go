package service

import (
	"context"
	"crypto/rand"
	"encoding/json"
	"strings"
	"time"

	"github.com/oklog/ulid/v2"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/domain"
)

type IngestionService struct {
	apiKeyValidator domain.APIKeyValidator
	logPublisher    domain.LogPublisher
}

func NewIngestionService(
	apiKeyValidator domain.APIKeyValidator,
	logPublisher domain.LogPublisher,
) *IngestionService {
	return &IngestionService{
		apiKeyValidator: apiKeyValidator,
		logPublisher:    logPublisher,
	}
}

func (s *IngestionService) AcceptLog(
	ctx context.Context,
	apiKey string,
	request domain.LogRequest,
) (*domain.APIKeyMetadata, error) {

	metadata, err := s.apiKeyValidator.Validate(ctx, apiKey)
	if err != nil {
		return nil, err
	}

	if strings.TrimSpace(request.Service) == "" ||
		strings.TrimSpace(request.Level) == "" ||
		strings.TrimSpace(request.Message) == "" {
		return nil, domain.ErrInvalidLogPayload
	}

	receivedAt := time.Now().UTC().Format(time.RFC3339Nano)

	timestamp := strings.TrimSpace(request.Timestamp)
	if timestamp == "" {
		timestamp = receivedAt
	}

	eventID := ulid.MustNew(ulid.Timestamp(time.Now()), rand.Reader).String()

	event := domain.LogEvent{
		EventID:       eventID,
		Timestamp:     timestamp,
		ReceivedAt:    receivedAt,
		WorkspaceID:   metadata.WorkspaceID,
		ProjectID:     metadata.ProjectID,
		ApplicationID: metadata.ApplicationID,
		Environment:   metadata.Environment,
		Service:       strings.TrimSpace(request.Service),
		Level:         strings.ToUpper(strings.TrimSpace(request.Level)),
		Message:       strings.TrimSpace(request.Message),
		TraceID:       strings.TrimSpace(request.TraceID),
		CorrelationID: strings.TrimSpace(request.CorrelationID),
		Metadata:      request.Metadata,
	}

	payload, err := json.Marshal(event)
	if err != nil {
		return nil, err
	}

	if err := s.logPublisher.Publish(ctx, payload); err != nil {
		return nil, err
	}

	return metadata, nil
}
