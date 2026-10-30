# Health Checks

Health check giúp biết service có sẵn sàng nhận traffic hay không.

## Loại health check

- Liveness: process còn sống.
- Readiness: service có thể phục vụ request.
- Dependency health: Redis, Kafka topic và Control API validation endpoint cho Ingestion API.

## Endpoint hiện có

- Control API: `GET /health`.
- Ingestion API liveness: `GET /health/live`; chỉ kiểm tra process của Ingestion API.
- Ingestion API readiness: `GET /health/ready`; kiểm tra Redis, Kafka topic có partitions dùng được và Internal Control API validation.

## Nguyên tắc

Readiness phải fail nếu dependency bắt buộc không sẵn sàng. Liveness không nên phụ thuộc network để tránh restart loop không cần thiết. Log Processor hiện không expose HTTP health endpoint; khởi động thất bại khi Kafka/OpenSearch mapping bắt buộc không sẵn sàng được quan sát qua container state và logs.
