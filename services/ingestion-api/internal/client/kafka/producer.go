package kafka

import (
	"context"

	confluent "github.com/confluentinc/confluent-kafka-go/v2/kafka"
)

type Producer struct {
	producer *confluent.Producer
	topic    string
}

func NewProducer(brokers string, topic string) (*Producer, error) {
	p, err := confluent.NewProducer(&confluent.ConfigMap{
		"bootstrap.servers": brokers,
	})

	if err != nil {
		return nil, err
	}

	return &Producer{
		producer: p,
		topic:    topic,
	}, nil
}

func (k *Producer) Publish(ctx context.Context, value []byte) error {
	if err := ctx.Err(); err != nil {
		return err
	}

	return k.producer.Produce(
		&confluent.Message{
			TopicPartition: confluent.TopicPartition{
				Topic:     &k.topic,
				Partition: confluent.PartitionAny,
			},
			Value: value,
		},
		nil,
	)
}

func (k *Producer) HealthCheck() error {
	_, err := k.producer.GetMetadata(nil, true, 200)
	return err
}
