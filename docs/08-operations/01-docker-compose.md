# Docker Compose

Docker Compose dùng để chạy local stack của TraceFlow.

## Service dự kiến

- `postgres`
- `kafka`
- `opensearch`
- `opensearch-dashboards`
- `control-api`
- `ingestion-api`
- `log-processor`
- `redis`

## Nguyên tắc

Mỗi service cần health check và environment rõ ràng. Volume local phải được đặt tên để tránh mất dữ liệu ngoài ý muốn.
