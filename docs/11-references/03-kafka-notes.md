# Kafka Notes

## Khái niệm

- Topic.
- Partition.
- Producer.
- Consumer.
- Consumer group.
- Offset.
- Consumer lag.

## Trong TraceFlow

Kafka tách ingestion khỏi processing. Ingestion publish log event, processor consume theo consumer group và index vào OpenSearch.

## Cần quyết định

- Số partition.
- Event key.
- Retention của topic.
- Retry topic hoặc DLQ.
- Offset commit strategy.
