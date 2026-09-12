package main

import (
	"log"
	"net/http"
	"time"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/adapters/controlapi"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/adapters/kafka"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/application/ingestion"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/config"
	transporthttp "github.com/T-Hank2712/traceflow/ingestion-api/internal/transport/http"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/transport/http/middleware"
)

func main() {
	cfg := config.Load()

	policy := ingestion.NewPolicy(
		cfg.MaxBatchSize,
		cfg.MaxRequestBodyBytes,
	)

	controlAPIClient := controlapi.NewClient(
		cfg.ControlAPIBaseURL,
		cfg.InternalServiceSecret,
		time.Duration(cfg.ControlAPITimeoutSeconds)*time.Second,
	)

	kafkaProducer, err := kafka.NewProducer(
		cfg.KafkaBootstrapServers,
		cfg.KafkaTopic,
	)
	if err != nil {
		log.Fatalf("Failed to create Kafka producer: %v", err)
	}

	ingestionService := ingestion.NewIngestionService(
		controlAPIClient,
		kafkaProducer,
		policy,
	)

	router := transporthttp.NewRouter(
		kafkaProducer,
		ingestionService,
		policy,
	)

	log.Printf("Ingestion API listening on %s", cfg.Port)

	if err := http.ListenAndServe(cfg.Port, middleware.Logging(router)); err != nil {
		log.Fatal(err)
	}
}
