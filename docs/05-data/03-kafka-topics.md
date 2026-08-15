# Kafka Topics

Kafka là buffer giữa ingestion và processing.

## Topic chính

```text
traceflow.logs.ingested
```

## Topic lỗi và mở rộng

```text
traceflow.logs.dlq
traceflow.alerts.events
traceflow.usage.events
```

## Event key

Event key có thể là `project_id` hoặc `application_id` tùy chiến lược partition. Quyết định này ảnh hưởng tới ordering và phân phối tải.

## Consumer group

Go Log Processor dùng consumer group để scale ngang.
