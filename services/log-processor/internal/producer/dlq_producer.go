package producer

import (
	"context"
	"encoding/json"
	"fmt"

	"github.com/T-Hank2712/traceflow/log-processor/internal/model"
	"github.com/confluentinc/confluent-kafka-go/v2/kafka"
)

type DLQProducer struct {
	producer *kafka.Producer
	topic    string
}

func NewDLQProducer(brokers string, topic string) (*DLQProducer, error) {
	p, err := kafka.NewProducer(&kafka.ConfigMap{
		"bootstrap.servers": brokers,
	})

	if err != nil {
		return nil, err
	}

	return &DLQProducer{
		producer: p,
		topic:    topic,
	}, nil
}

func (p *DLQProducer) Publish(ctx context.Context, event model.DeadLetterEvent) error {
	if err := ctx.Err(); err != nil {
		return err
	}

	payload, err := json.Marshal(event)
	if err != nil {
		return fmt.Errorf("failed to marshal dead letter event: %w", err)
	}

	return p.producer.Produce(
		&kafka.Message{
			TopicPartition: kafka.TopicPartition{
				Topic:     &p.topic,
				Partition: kafka.PartitionAny,
			},
			Value: payload,
		},
		nil,
	)
}
