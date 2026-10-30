# Docker Compose

Docker Compose dùng để chạy local stack của TraceFlow.

## Service hiện có

- `postgres`
- `kafka`
- `opensearch`
- `redis`
- `control-api`
- `ingestion-api`
- `log-processor`
- `nginx`
- `kafka-init` để tạo `traceflow.logs` và `traceflow.logs.dlq`

Performance Compose bổ sung `network-simulator`, `load-generator` và `k6`.

Nginx là entry point public: local stack expose `80`/`443`; performance stack expose `8080`/`8443`. Control API và Ingestion API không publish port trực tiếp ra host trong Compose mặc định.

## Nguyên tắc

Mỗi service cần health check và environment rõ ràng. Volume local phải được đặt tên để tránh mất dữ liệu ngoài ý muốn. `down -v` xóa PostgreSQL, Redis, Kafka và OpenSearch data của stack; chỉ dùng khi chủ động reset môi trường.
