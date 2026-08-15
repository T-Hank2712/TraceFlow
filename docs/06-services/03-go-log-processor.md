# Go Log Processor

Go Log Processor consume log event từ Kafka, xử lý batch và index vào OpenSearch.

## Trách nhiệm

- Consume Kafka theo consumer group.
- Validate internal event.
- Enrich dữ liệu với `processed_at`.
- Gom batch theo size hoặc interval.
- Bulk index vào OpenSearch.
- Retry, DLQ và graceful shutdown.
- Export metrics về throughput, lag và indexing latency.

## Nguyên tắc

Offset chỉ được commit sau khi event đã được xử lý theo policy. Với lỗi không thể xử lý, event phải được đưa vào DLQ thay vì mất im lặng.
