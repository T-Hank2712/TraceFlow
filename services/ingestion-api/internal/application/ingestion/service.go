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
	policy          Policy
}

func NewIngestionService(
	apiKeyValidator ports.APIKeyValidator,
	logPublisher ports.LogPublisher,
	policy Policy,
) *IngestionService {
	return &IngestionService{
		apiKeyValidator: apiKeyValidator,
		logPublisher:    logPublisher,
		policy:          policy,
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

	if err := validateLogRequest(request, s.policy); err != nil {
		return nil, err
	}

	if err := s.publishLog(ctx, metadata, request); err != nil {
		return nil, err
	}

	return metadata, nil
}

func (s *IngestionService) AcceptBatchLogs(
	ctx context.Context,
	apiKey string,
	request domain.BatchLogRequest,
) (*domain.APIKeyMetadata, int, error) {
	metadata, err := s.apiKeyValidator.Validate(ctx, apiKey)
	if err != nil {
		return nil, 0, err
	}

	if err := validateBatchLogRequest(request, s.policy); err != nil {
		return nil, 0, err
	}

	publishedCount := 0

	for index, logRequest := range request.Logs {
		if err := s.publishLog(ctx, metadata, logRequest); err != nil {
			return nil, publishedCount, domain.BatchPublishError{
				FailedIndex:    index,
				PublishedCount: publishedCount,
				TotalCount:     len(request.Logs),
				Cause:          err,
			}
		}

		publishedCount++
	}

	return metadata, len(request.Logs), nil
}

func (s *IngestionService) publishLog(
	ctx context.Context,
	metadata *domain.APIKeyMetadata,
	request domain.LogRequest,
) error {
	event := newLogEvent(metadata, request)

	payload, err := json.Marshal(event)
	if err != nil {
		return err
	}

	return s.logPublisher.Publish(ctx, payload)
}
