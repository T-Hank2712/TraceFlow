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

	cfg := config.Load()

	openSearchRepository, err := repository.NewOpenSearchRepository(
		cfg.OpenSearchURL,
		cfg.OpenSearchUsername,
		cfg.OpenSearchPassword,
		cfg.OpenSearchIndex,
		cfg.OpenSearchSkipTLSVerify,
	)

	if err != nil {
		log.Fatalf("Failed to create OpenSearch repository: %v", err)
	}

	logService := service.NewLogService(openSearchRepository)

	kafkaConsumer, err := consumer.NewKafkaConsumer(
		cfg.KafkaBootstrapServers,
		cfg.KafkaConsumerGroup,
		cfg.KafkaTopic,
		logService,
	)

	if err != nil {
		log.Fatalf("Failed to create Kafka consumer: %v", err)
	}

	if err := kafkaConsumer.Subscribe(); err != nil {
		log.Fatalf("Failed to start Kafka consumer: %v", err)
	}
}
