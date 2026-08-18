package consumer

import (
	"encoding/json"
	"log"

	"github.com/T-Hank2712/traceflow/log-processor/internal/model"
	"github.com/confluentinc/confluent-kafka-go/v2/kafka"
)

type KafkaConsumer struct {
	consumer *kafka.Consumer
	topic    string
}

func NewKafkaConsumer(brokers string, groupID string, topic string) (*KafkaConsumer, error) {
	c, err := kafka.NewConsumer(&kafka.ConfigMap{
		"bootstrap.servers": brokers,
		"group.id":          groupID,
		"auto.offset.reset": "earliest",
	})

	if err != nil {
		return nil, err
	}

	return &KafkaConsumer{
		consumer: c,
		topic:    topic,
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
		message, err := k.consumer.ReadMessage(-1)
		if err != nil {
			log.Printf("Failed to read Kafka message: %v", err)
			continue
		}
		var event model.LogEvent

		if err := json.Unmarshal(message.Value, &event); err != nil {
			log.Printf("Failed to unmarshal message: %v", err)
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
