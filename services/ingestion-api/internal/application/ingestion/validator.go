package ingestion

import (
	"strings"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/domain"
)

func validateLogRequest(
	request domain.LogRequest,
	policy Policy,
) error {
	if strings.TrimSpace(request.Service) == "" ||
		strings.TrimSpace(request.Level) == "" ||
		strings.TrimSpace(request.Message) == "" {
		return domain.ErrInvalidLogPayload
	}

	level := strings.ToUpper(strings.TrimSpace(request.Level))
	if _, ok := policy.AllowedLevels[level]; !ok {
		return domain.ErrInvalidLogPayload
	}

	return nil
}

func validateBatchLogRequest(
	request domain.BatchLogRequest,
	policy Policy,
) error {
	if len(request.Logs) == 0 {
		return domain.ErrInvalidLogPayload
	}

	if len(request.Logs) > policy.MaxBatchSize {
		return domain.ErrInvalidLogPayload
	}

	for _, logRequest := range request.Logs {
		if err := validateLogRequest(logRequest, policy); err != nil {
			return err
		}
	}

	return nil
}
