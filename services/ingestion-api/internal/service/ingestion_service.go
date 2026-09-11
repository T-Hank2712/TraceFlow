package service

import (
	"context"
	"encoding/json"

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

	event := domain.LogEvent{
		Service: request.Service,
		Level:   request.Level,
		Message: request.Message,
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
