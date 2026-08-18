package main

import (
	"log"
	"net/http"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/handler"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/producer"
)

func main() {

	kafkaProducer, err := producer.NewKafkaProducer(
		"localhost:9092",
		"traceflow.logs",
	)
	if err != nil {
		log.Fatalf("Failed to create Kafka producer: %v", err)
	}

	logHandler := handler.NewLogHandler(kafkaProducer)

	http.HandleFunc("/logs", logHandler.Handle)

	http.HandleFunc("/health", handler.HealthHandler)

	log.Println("Ingestion API listening on :8080")
	if err := http.ListenAndServe(":8080", nil); err != nil {
		log.Fatal(err)
	}
}
