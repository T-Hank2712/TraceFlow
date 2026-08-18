package main

import (
	"log"
	"net/http"

	"github.com/T-Hank2712/traceflow/ingestion-api/config"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/handler"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/producer"
	"github.com/T-Hank2712/traceflow/ingestion-api/middleware"
)

func main() {
	cfg := config.Load()
	kafkaProducer, err := producer.NewKafkaProducer(
		cfg.KafkaBootstrapServers,
		cfg.KafkaTopic,
	)
	log.Print(cfg.KafkaTopic)
	log.Print(cfg.KafkaBootstrapServers)
	if err != nil {
		log.Fatalf("Failed to create Kafka producer: %v", err)
	}

	logHandler := handler.NewLogHandler(kafkaProducer)

	mux := http.NewServeMux()

	mux.HandleFunc("/health", handler.HealthHandler)
	mux.HandleFunc("/logs", logHandler.Handle)

	log.Println("Ingestion API listening on :8080")

	if err := http.ListenAndServe(":8080", middleware.Logging(mux)); err != nil {
		log.Fatal(err)
	}
}
