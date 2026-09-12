package main

import (
	"log"
	"net/http"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/adapters/controlapi"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/adapters/kafka"
	ingestionservice "github.com/T-Hank2712/traceflow/ingestion-api/internal/application/ingestion"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/config"
	transporthttp "github.com/T-Hank2712/traceflow/ingestion-api/internal/transport/http"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/transport/http/middleware"
)

func main() {
	cfg := config.Load()
	controlAPIClient := controlapi.NewClient(
		cfg.ControlAPIBaseURL,
		cfg.InternalServiceSecret,
	)
	kafkaProducer, err := kafka.NewProducer(
		cfg.KafkaBootstrapServers,
		cfg.KafkaTopic,
	)
	if err != nil {
		log.Fatalf("Failed to create Kafka producer: %v", err)
	}

	ingestionService := ingestionservice.NewIngestionService(
		controlAPIClient,
		kafkaProducer,
	)

	router := transporthttp.NewRouter(kafkaProducer, ingestionService)

	log.Println("Ingestion API listening on :8080")

	if err := http.ListenAndServe(":8080", middleware.Logging(router)); err != nil {
		log.Fatal(err)
	}
}
