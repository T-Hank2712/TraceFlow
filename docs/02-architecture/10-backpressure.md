# Backpressure

Backpressure xảy ra khi tốc độ ingest lớn hơn tốc độ xử lý/index.

## Tín hiệu cần theo dõi

- Kafka consumer lag.
- Throughput của ingestion.
- Throughput của processor.
- OpenSearch indexing latency.
- Memory usage của processor.
- Số lỗi retry hoặc timeout.

## Nguyên tắc

- Không gom log trong memory vô hạn.
- Batch size phải có giới hạn.
- Request body batch phải có giới hạn số log và dung lượng.
- Khi Kafka hoặc downstream quá tải, ingestion cần trả lỗi hoặc rate limit rõ ràng.

## Thiết kế hoàn chỉnh

Hệ thống cần kết hợp giới hạn batch, timeout, rate limiting, consumer autoscaling theo lag và cảnh báo khi downstream chậm. Khi vượt ngưỡng an toàn, ingestion phải trả lỗi hoặc giảm tốc rõ ràng thay vì làm đầy memory.
