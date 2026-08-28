package consumer

import (
	"encoding/json"
	"log"
	"time"

	"github.com/T-Hank2712/traceflow/log-processor/internal/model"
	"github.com/T-Hank2712/traceflow/log-processor/internal/service"
	"github.com/confluentinc/confluent-kafka-go/v2/kafka"
)

type KafkaConsumer struct {
	consumer   *kafka.Consumer
	topic      string
	logService *service.LogService
}

func NewKafkaConsumer(brokers string, groupID string, topic string, logService *service.LogService) (*KafkaConsumer, error) {
	c, err := kafka.NewConsumer(&kafka.ConfigMap{
		"bootstrap.servers": brokers,
		"group.id":          groupID,
		"auto.offset.reset": "earliest",
	})

	if err != nil {
		return nil, err
	}

	return &KafkaConsumer{
		consumer:   c,
		topic:      topic,
		logService: logService,
	}, nil
}

func (k *KafkaConsumer) Subscribe() error {
	if err := k.consumer.SubscribeTopics(
		[]string{k.topic},
		nil,
	); err != nil {
		return err
	}

	log.Printf("Kafka consumer subscribed to topic: %s", k.topic)

	for {
		log.Println("Waiting for Kafka message...")
		message, err := k.consumer.ReadMessage(-1)
		log.Printf("Received Kafka message at %s", time.Now().Format(time.RFC3339Nano))
		if err != nil {
			log.Printf("Failed to read Kafka message: %v", err)
			continue
		}
		var event model.LogEvent

		if err := json.Unmarshal(message.Value, &event); err != nil {
			log.Printf("Failed to unmarshal message: %v", err)
			continue
		}

		if err := k.logService.Process(&event); err != nil {
			log.Printf("Failed to process log event: %v", err)
			continue
		}

		log.Printf(
			"Processed log: service=%s level=%s message=%s",
			event.Service,
			event.Level,
			event.Message,
		)
	}
}
