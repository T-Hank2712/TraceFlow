# Kafka Topics

Kafka là buffer giữa ingestion và processing.

## Topic chính

```text
traceflow.logs.ingested
```

## Topic lỗi

```text
traceflow.logs.dlq
```

## Topic mở rộng nếu cần

```text
traceflow.usage.events
traceflow.alerts.events
```

`traceflow.usage.events` chỉ cần khi triển khai quota/usage nâng cao. `traceflow.alerts.events` thuộc optional alerting.

## Event key

Event key mặc định nên là `application_id` để cân bằng throughput và giữ ordering tương đối theo application. Nếu strategy thay đổi, cần cập nhật system design và reliability docs.

## Consumer group

Log Processor dùng consumer group để scale ngang.
