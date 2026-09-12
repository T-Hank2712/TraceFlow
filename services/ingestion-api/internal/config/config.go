package config

import (
	"log"
	"os"
	"strconv"

	"github.com/joho/godotenv"
)

type Config struct {
	Port string

	ControlAPIBaseURL        string
	InternalServiceSecret    string
	ControlAPITimeoutSeconds int

	KafkaBootstrapServers string
	KafkaTopic            string

	MaxBatchSize        int
	MaxRequestBodyBytes int64
}

func Load() Config {
	if err := godotenv.Load(); err != nil {
		log.Println("No .env file found, using system environment variables")
	}

	return Config{
		Port: getString("PORT", ":8080"),

		ControlAPIBaseURL:        getString("CONTROL_API_BASE_URL", "http://localhost:5075"),
		InternalServiceSecret:    os.Getenv("INTERNAL_SERVICE_SECRET"),
		ControlAPITimeoutSeconds: getInt("CONTROL_API_TIMEOUT_SECONDS", 5),

		KafkaBootstrapServers: getString("KAFKA_BOOTSTRAP_SERVERS", "localhost:9092"),
		KafkaTopic:            getString("KAFKA_TOPIC", "traceflow.logs"),

		MaxBatchSize:        getInt("MAX_BATCH_SIZE", 100),
		MaxRequestBodyBytes: getInt64("MAX_REQUEST_BODY_BYTES", 1048576),
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

func getInt64(key string, defaultValue int64) int64 {
	value := os.Getenv(key)
	if value == "" {
		return defaultValue
	}

	parsed, err := strconv.ParseInt(value, 10, 64)
	if err != nil {
		return defaultValue
	}

	return parsed
}
