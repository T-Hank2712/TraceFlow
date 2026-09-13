package ingestion

import "github.com/T-Hank2712/traceflow/ingestion-api/internal/constants"

type Policy struct {
	MaxBatchSize        int
	MaxRequestBodyBytes int64
	AllowedLevels       map[string]struct{}
}

func NewPolicy(
	maxBatchSize int,
	maxRequestBodyBytes int64,
) Policy {
	return Policy{
		MaxBatchSize:        maxBatchSize,
		MaxRequestBodyBytes: maxRequestBodyBytes,
		AllowedLevels: map[string]struct{}{
			constants.LogLevelDebug: {},
			constants.LogLevelInfo:  {},
			constants.LogLevelWarn:  {},
			constants.LogLevelError: {},
			constants.LogLevelFatal: {},
		},
	}
}
