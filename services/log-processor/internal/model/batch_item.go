package model

import "github.com/confluentinc/confluent-kafka-go/v2/kafka"

type BatchItem struct {
	Event   LogEvent
	Message *kafka.Message
}
