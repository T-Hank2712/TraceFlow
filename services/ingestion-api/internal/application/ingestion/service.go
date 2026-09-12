package ingestion

import (
	"context"
	"encoding/json"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/domain"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/ports"
)

type IngestionService struct {
	apiKeyValidator ports.APIKeyValidator
	logPublisher    ports.LogPublisher
}

func NewIngestionService(
	apiKeyValidator ports.APIKeyValidator,
	logPublisher ports.LogPublisher,
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

	if err := validateLogRequest(request); err != nil {
		return nil, err
	}

	event := newLogEvent(metadata, request)

	payload, err := json.Marshal(event)
	if err != nil {
		return nil, err
	}

	if err := s.logPublisher.Publish(ctx, payload); err != nil {
		return nil, err
	}

	return metadata, nil
}
