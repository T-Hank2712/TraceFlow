package ingestion

import (
	"strings"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/domain"
)

func validateLogRequest(request domain.LogRequest) error {
	if strings.TrimSpace(request.Service) == "" ||
		strings.TrimSpace(request.Level) == "" ||
		strings.TrimSpace(request.Message) == "" {
		return domain.ErrInvalidLogPayload
	}

	return nil
}
