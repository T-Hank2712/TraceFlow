package kafka

import (
	"context"
	"fmt"
	"time"

	confluent "github.com/confluentinc/confluent-kafka-go/v2/kafka"
)

type Producer struct {
	producer        *confluent.Producer
	topic           string
	deliveryTimeout time.Duration
}

func NewProducer(brokers string, topic string, deliveryTimeout time.Duration) (*Producer, error) {
	p, err := confluent.NewProducer(&confluent.ConfigMap{
		"bootstrap.servers": brokers,
	})

	if err != nil {
		return nil, err
	}

	return &Producer{
		producer:        p,
		topic:           topic,
		deliveryTimeout: deliveryTimeout,
	}, nil
}

func (k *Producer) Publish(ctx context.Context, value []byte) error {
	if err := ctx.Err(); err != nil {
		return err
	}

	deliveryChan := make(chan confluent.Event, 1)

	message := &confluent.Message{
		TopicPartition: confluent.TopicPartition{
			Topic:     &k.topic,
			Partition: confluent.PartitionAny,
		},
		Value: value,
	}

	if err := k.producer.Produce(message, deliveryChan); err != nil {
		return err
	}

	timeout := time.NewTimer(k.deliveryTimeout)
	defer timeout.Stop()

	select {
	case event := <-deliveryChan:
		deliveredMessage, ok := event.(*confluent.Message)
		if !ok {
			return fmt.Errorf("unexpected kafka delivery event")
		}

		if deliveredMessage.TopicPartition.Error != nil {
			return deliveredMessage.TopicPartition.Error
		}

		return nil

	case <-ctx.Done():
		return ctx.Err()

	case <-timeout.C:
		return fmt.Errorf("kafka delivery timed out")
	}
}

func (k *Producer) HealthCheck() error {
	_, err := k.producer.GetMetadata(nil, true, 200)
	return err
}
