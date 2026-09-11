package config

import (
	"log"
	"os"

	"github.com/joho/godotenv"
)

type Config struct {
	KafkaBootstrapServers string
	KafkaTopic            string
	ControlAPIBaseURL     string
	InternalServiceSecret string
}

func Load() Config {
	if err := godotenv.Load(); err != nil {
		log.Println("No .env file found, using system environment variables")
	}

	return Config{
		KafkaBootstrapServers: os.Getenv("KAFKA_BOOTSTRAP_SERVERS"),
		KafkaTopic:            os.Getenv("KAFKA_TOPIC"),
		ControlAPIBaseURL:     os.Getenv("CONTROL_API_BASE_URL"),
		InternalServiceSecret: os.Getenv("INTERNAL_SERVICE_SECRET"),
	}
}
