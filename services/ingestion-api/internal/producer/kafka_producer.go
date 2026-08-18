package producer

import (
	"github.com/confluentinc/confluent-kafka-go/v2/kafka"
)

type KafkaProducer struct {
	producer *kafka.Producer
	topic    string
}

func NewKafkaProducer(brokers string, topic string) (*KafkaProducer, error) {
	p, err := kafka.NewProducer(&kafka.ConfigMap{
		"bootstrap.servers": brokers,
	})

	if err != nil {
		return nil, err
	}

	return &KafkaProducer{
		producer: p,
		topic:    topic,
	}, nil
}
