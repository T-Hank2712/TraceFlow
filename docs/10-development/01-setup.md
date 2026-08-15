# Development Setup

Tài liệu này hướng dẫn chuẩn bị môi trường phát triển TraceFlow.

## Công cụ cần có

- Go.
- .NET SDK.
- Docker.
- Docker Compose.
- PostgreSQL client nếu cần.
- OpenSearch client hoặc curl.

## Luồng setup

1. Copy `.env.example` thành `.env`.
2. Chạy Docker Compose.
3. Chạy migration.
4. Seed sample data.
5. Gửi log thử qua Ingestion API.
6. Search log qua Control API.
