# Local Development Stack

Local stack giúp chạy TraceFlow trên máy phát triển bằng Docker Compose.

## Thành phần

- PostgreSQL.
- Kafka.
- OpenSearch.
- OpenSearch Dashboards nếu cần debug index.
- ASP.NET Core Control API.
- ASP.NET Core Ingestion API.
- .NET Log Processor.
- Redis cho validation cache, rate limiting và counter ngắn hạn.

## Nguyên tắc

Local stack phải có health check, cấu hình port rõ ràng và sample data để kiểm tra end-to-end flow.
