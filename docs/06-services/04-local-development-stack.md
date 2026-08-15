# Local Development Stack

Local stack giúp chạy TraceFlow trên máy phát triển bằng Docker Compose.

## Thành phần

- PostgreSQL.
- Kafka.
- OpenSearch.
- OpenSearch Dashboards nếu cần debug index.
- ASP.NET Core Control API.
- Go Ingestion Service.
- Go Log Processor.
- Redis nếu bật rate limiting/caching.

## Nguyên tắc

Local stack phải có health check, cấu hình port rõ ràng và sample data để kiểm tra end-to-end flow.
