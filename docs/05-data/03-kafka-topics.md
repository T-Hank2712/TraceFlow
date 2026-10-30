# Kafka Topics

Kafka là buffer giữa ingestion và processing.

## Topic chính

```text
traceflow.logs
```

## Topic lỗi

```text
traceflow.logs.dlq
```

Docker Compose hiện tạo `traceflow.logs` với 3 partitions và `traceflow.logs.dlq` với 1 partition, replication factor 1. Đây là cấu hình local/performance single-broker, không phải khuyến nghị production cluster.

## Topic mở rộng nếu cần

```text
traceflow.usage.events
traceflow.alerts.events
```

`traceflow.usage.events` chỉ cần khi triển khai quota/usage nâng cao. `traceflow.alerts.events` thuộc optional alerting.

## Event key

Producer hiện dùng `eventId` làm message key. Vì vậy Kafka chỉ đảm bảo ordering theo event key/partition, không phải theo application.

## Consumer group

Log Processor dùng consumer group để scale ngang.
