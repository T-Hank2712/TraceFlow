package main

import (
	"log"

	"github.com/T-Hank2712/traceflow/log-processor/internal/consumer"
)

func main() {
	log.Println("Log Processor started")

	kafkaConsumer, err := consumer.NewKafkaConsumer(
		"localhost:9092",
		"traceflow-log-processor",
		"traceflow.logs",
	)

	if err != nil {
		log.Fatalf("Failed to create Kafka consumer: %v", err)
	}

	if err := kafkaConsumer.Subscribe(); err != nil {
		log.Fatalf("Failed to start Kafka consumer: %v", err)
	}
}
