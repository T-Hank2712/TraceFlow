package http

import (
	"net/http"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/application/ingestion"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/constants"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/domain"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/ports"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/transport/http/handler"
)

func NewRouter(
	kafkaHealthChecker ports.HealthChecker,
	ingestionService domain.IngestionService,
	policy ingestion.Policy,
) http.Handler {
	logHandler := handler.NewLogHandler(
		ingestionService,
		policy,
	)

	mux := http.NewServeMux()
	mux.HandleFunc(constants.HealthRoute, handler.HealthHandler)
	mux.HandleFunc(constants.KafkaHealthRoute, handler.KafkaHealthHandler(kafkaHealthChecker))
	mux.HandleFunc(constants.BatchLogsRoute, logHandler.HandleBatch)
	mux.HandleFunc(constants.LogsRoute, logHandler.Handle)

	return mux
}
