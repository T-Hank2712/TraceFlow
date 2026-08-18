package main

import (
	"log"

	"github.com/T-Hank2712/traceflow/log-processor/internal/consumer"
	"github.com/T-Hank2712/traceflow/log-processor/internal/service"
)

func main() {
	log.Println("Log Processor started")

	logService := service.NewLogService()

	kafkaConsumer, err := consumer.NewKafkaConsumer(
		"localhost:9092",
		"traceflow-log-processor",
		"traceflow.logs",
		logService,
	)

	log.Println(logService)

	if err != nil {
		log.Fatalf("Failed to create Kafka consumer: %v", err)
	}

	if err := kafkaConsumer.Subscribe(); err != nil {
		log.Fatalf("Failed to start Kafka consumer: %v", err)
	}
}
