package main

import (
	"log"

	"github.com/T-Hank2712/traceflow/log-processor/internal/config"
	"github.com/T-Hank2712/traceflow/log-processor/internal/consumer"
	"github.com/T-Hank2712/traceflow/log-processor/internal/repository"
	"github.com/T-Hank2712/traceflow/log-processor/internal/service"
)

func main() {
	log.Println("Log Processor started")

	OpenSearchRepository, err := repository.NewOpenSearchRepository(
		config.Load().OpenSearchURL,
		config.Load().OpenSearchUsername,
		config.Load().OpenSearchPassword,
		config.Load().OpenSearchIndex,
	)

	if err != nil {
		log.Fatalf("Failed to create OpenSearch repository: %v", err)
	}

	logService := service.NewLogService(OpenSearchRepository)
	cfg := config.Load()

	kafkaConsumer, err := consumer.NewKafkaConsumer(
		cfg.KafkaBootstrapServers,
		cfg.KafkaConsumerGroup,
		cfg.KafkaTopic,
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
