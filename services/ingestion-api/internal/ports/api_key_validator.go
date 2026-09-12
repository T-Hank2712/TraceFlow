package ports

import (
	"context"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/domain"
)

type APIKeyValidator interface {
	Validate(ctx context.Context, apiKey string) (*domain.APIKeyMetadata, error)
}
