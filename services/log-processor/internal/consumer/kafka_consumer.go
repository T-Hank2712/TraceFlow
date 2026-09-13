package consumer

import (
	"context"
	"encoding/json"
	"errors"
	"log"
	"time"

	"github.com/T-Hank2712/traceflow/log-processor/internal/model"
	"github.com/T-Hank2712/traceflow/log-processor/internal/producer"
	"github.com/T-Hank2712/traceflow/log-processor/internal/service"
	"github.com/confluentinc/confluent-kafka-go/v2/kafka"
)

type KafkaConsumer struct {
	consumer                *kafka.Consumer
	topic                   string
	logService              *service.LogService
	dlqProducer             *producer.DLQProducer
	processorMaxRetries     int
	processorRetryBackoffMs int
	batch                   []model.BatchItem
	batchSize               int
	flushInterval           time.Duration
}

func NewKafkaConsumer(
	brokers string,
	groupID string,
	topic string,
	logService *service.LogService,
	dlqProducer *producer.DLQProducer,
	processorMaxRetries int,
	processorRetryBackoffMs int,
	batchSize int,
	flushIntervalMs int,
) (*KafkaConsumer, error) {
	c, err := kafka.NewConsumer(&kafka.ConfigMap{
		"bootstrap.servers": brokers,
		"group.id":          groupID,
		"auto.offset.reset": "earliest",
	})

	if err != nil {
		return nil, err
	}

	return &KafkaConsumer{
		consumer:                c,
		topic:                   topic,
		logService:              logService,
		dlqProducer:             dlqProducer,
		processorMaxRetries:     processorMaxRetries,
		processorRetryBackoffMs: processorRetryBackoffMs,
		batch:                   make([]model.BatchItem, 0, batchSize),
		batchSize:               batchSize,
		flushInterval:           time.Duration(flushIntervalMs) * time.Millisecond,
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

			if dlqErr := k.publishDeadLetter(
				context.Background(),
				message,
				model.FailureStageJSONDecode,
				err.Error(),
				nil,
			); dlqErr != nil {
				log.Printf("Failed to publish invalid JSON message to DLQ: %v", dlqErr)
			}

			continue
		}

		if err := k.logService.Prepare(&event); err != nil {
			log.Printf("Failed to prepare log event: %v", err)

			if errors.Is(err, service.ErrInvalidLogEvent) {
				if dlqErr := k.publishDeadLetter(
					context.Background(),
					message,
					model.FailureStageValidation,
					err.Error(),
					&event,
				); dlqErr != nil {
					log.Printf("Failed to publish invalid log event to DLQ: %v", dlqErr)
				} else {
					log.Printf("Invalid log event sent to DLQ: eventId=%s topic=%s partition=%d offset=%d",
						event.EventID,
						topicName(message),
						message.TopicPartition.Partition,
						message.TopicPartition.Offset,
					)
				}

				continue
			}

			log.Printf("Unexpected log preparation error: %v", err)
			continue
		}

		k.addToBatch(model.BatchItem{
			Event:   event,
			Message: message,
		})

		if k.shouldFlushBatch() {
			k.flushBatch()
		}

		continue
	}
}

func (k *KafkaConsumer) publishDeadLetter(
	ctx context.Context,
	message *kafka.Message,
	failureStage string,
	failureReason string,
	event *model.LogEvent,
) error {
	deadLetterEvent := model.DeadLetterEvent{
		FailedAt:        time.Now().UTC().Format(time.RFC3339Nano),
		SourceTopic:     topicName(message),
		Partition:       message.TopicPartition.Partition,
		Offset:          int64(message.TopicPartition.Offset),
		FailureStage:    failureStage,
		FailureReason:   failureReason,
		OriginalPayload: string(message.Value),
	}

	if event != nil {
		deadLetterEvent.EventID = event.EventID
		deadLetterEvent.WorkspaceID = event.WorkspaceID
		deadLetterEvent.ProjectID = event.ProjectID
		deadLetterEvent.ApplicationID = event.ApplicationID
	}
	return k.dlqProducer.Publish(ctx, deadLetterEvent)
}

func topicName(message *kafka.Message) string {
	if message.TopicPartition.Topic == nil {
		return ""
	}

	return *message.TopicPartition.Topic
}

func (k *KafkaConsumer) processWithRetry(event *model.LogEvent) error {
	var lastErr error

	attempts := k.processorMaxRetries + 1
	backoff := time.Duration(k.processorRetryBackoffMs) * time.Millisecond

	for attempt := 1; attempt <= attempts; attempt++ {
		err := k.logService.Process(event)
		if err == nil {
			return nil
		}

		if errors.Is(err, service.ErrInvalidLogEvent) {
			return err
		}

		if !errors.Is(err, service.ErrIndexLogFailed) {
			return err
		}

		lastErr = err

		if attempt == attempts {
			break
		}

		log.Printf(
			"Retrying log indexing: attempt=%d maxAttempts=%d eventId=%s error=%v",
			attempt,
			attempts,
			event.EventID,
			err,
		)

		time.Sleep(backoff)
	}

	return lastErr
}

func (k *KafkaConsumer) addToBatch(item model.BatchItem) {
	k.batch = append(k.batch, item)

	log.Printf(
		"Added log event to batch: eventId=%s batchSize=%d maxBatchSize=%d",
		item.Event.EventID,
		len(k.batch),
		k.batchSize,
	)
}

func (k *KafkaConsumer) shouldFlushBatch() bool {
	return k.batchSize > 0 && len(k.batch) >= k.batchSize
}

func (k *KafkaConsumer) flushBatch() {
	if len(k.batch) == 0 {
		return
	}

	batch := k.batch
	k.batch = make([]model.BatchItem, 0, k.batchSize)

	log.Printf("Flushing log batch: size=%d", len(batch))

	for _, item := range batch {
		if err := k.processWithRetry(&item.Event); err != nil {
			log.Printf("Failed to process batched log event: %v", err)

			if errors.Is(err, service.ErrInvalidLogEvent) {
				if dlqErr := k.publishDeadLetter(
					context.Background(),
					item.Message,
					model.FailureStageValidation,
					err.Error(),
					&item.Event,
				); dlqErr != nil {
					log.Printf("Failed to publish invalid batched log event to DLQ: %v", dlqErr)
				}

				continue
			}

			if errors.Is(err, service.ErrIndexLogFailed) {
				if dlqErr := k.publishDeadLetter(
					context.Background(),
					item.Message,
					model.FailureStageIndexing,
					err.Error(),
					&item.Event,
				); dlqErr != nil {
					log.Printf("Failed to publish batched indexing failure to DLQ: %v", dlqErr)
				}

				continue
			}

			log.Printf("Unexpected batched log processing error: %v", err)
			continue
		}
	}

	log.Printf("Flushed log batch: size=%d", len(batch))
}
