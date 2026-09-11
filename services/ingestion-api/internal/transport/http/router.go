package http

import (
	"net/http"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/client/kafka"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/domain"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/transport/http/handler"
)

func NewRouter(
	logPublisher *kafka.Producer,
	ingestionService domain.IngestionService,
) http.Handler {
	logHandler := handler.NewLogHandler(ingestionService)

	mux := http.NewServeMux()
	mux.HandleFunc("/health", handler.HealthHandler)
	mux.HandleFunc("/health/kafka", handler.KafkaHealthHandler(logPublisher))
	mux.HandleFunc("/logs", logHandler.Handle)

	return mux
}
