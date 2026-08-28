package config

import (
	"log"
	"os"

	"github.com/joho/godotenv"
)

type Config struct {
	KafkaBootstrapServers string
	KafkaConsumerGroup    string
	KafkaTopic            string

	OpenSearchURL      string
	OpenSearchUsername string
	OpenSearchPassword string
	OpenSearchIndex    string
}

func Load() Config {
	if err := godotenv.Load(); err != nil {
		log.Println("No .env file found, using system environment variables")
	}

	return Config{
		KafkaBootstrapServers: os.Getenv("KAFKA_BOOTSTRAP_SERVERS"),
		KafkaConsumerGroup:    os.Getenv("KAFKA_CONSUMER_GROUP"),
		KafkaTopic:            os.Getenv("KAFKA_TOPIC"),
		OpenSearchURL:         os.Getenv("OPENSEARCH_URL"),
		OpenSearchUsername:    os.Getenv("OPENSEARCH_USERNAME"),
		OpenSearchPassword:    os.Getenv("OPENSEARCH_PASSWORD"),
		OpenSearchIndex:       os.Getenv("OPENSEARCH_INDEX"),
	}
}
