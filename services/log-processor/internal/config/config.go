package config

import (
	"log"
	"os"
	"strconv"

	"github.com/joho/godotenv"
)

type Config struct {
	KafkaBootstrapServers string
	KafkaConsumerGroup    string
	KafkaTopic            string
	KafkaDLQTopic         string

	ProcessorMaxRetries      int
	ProcessorRetryBackoffMs  int
	ProcessorBatchSize       int
	ProcessorFlushIntervalMs int

	OpenSearchURL           string
	OpenSearchUsername      string
	OpenSearchPassword      string
	OpenSearchIndex         string
	OpenSearchSkipTLSVerify bool
}

func Load() Config {
	if err := godotenv.Load(); err != nil {
		log.Println("No .env file found, using system environment variables")
	}

	skipTLSVerify, _ := strconv.ParseBool(os.Getenv("OPENSEARCH_SKIP_TLS_VERIFY"))

	return Config{
		KafkaBootstrapServers:    os.Getenv("KAFKA_BOOTSTRAP_SERVERS"),
		KafkaConsumerGroup:       os.Getenv("KAFKA_CONSUMER_GROUP"),
		KafkaTopic:               os.Getenv("KAFKA_TOPIC"),
		KafkaDLQTopic:            getString("KAFKA_DLQ_TOPIC", "traceflow.logs.dlq"),
		ProcessorMaxRetries:      getInt("PROCESSOR_MAX_RETRIES", 3),
		ProcessorRetryBackoffMs:  getInt("PROCESSOR_RETRY_BACKOFF_MS", 500),
		ProcessorBatchSize:       getInt("PROCESSOR_BATCH_SIZE", 100),
		ProcessorFlushIntervalMs: getInt("PROCESSOR_FLUSH_INTERVAL_MS", 1000),
		OpenSearchURL:            os.Getenv("OPENSEARCH_URL"),
		OpenSearchUsername:       os.Getenv("OPENSEARCH_USERNAME"),
		OpenSearchPassword:       os.Getenv("OPENSEARCH_PASSWORD"),
		OpenSearchIndex:          os.Getenv("OPENSEARCH_INDEX"),
		OpenSearchSkipTLSVerify:  skipTLSVerify,
	}
}
func getString(key string, defaultValue string) string {
	value := os.Getenv(key)
	if value == "" {
		return defaultValue
	}

	return value
}

func getInt(key string, defaultValue int) int {
	value := os.Getenv(key)
	if value == "" {
		return defaultValue
	}

	parsed, err := strconv.Atoi(value)
	if err != nil {
		return defaultValue
	}

	return parsed
}
