# Services

Thư mục này chứa các service runtime của TraceFlow.

TraceFlow chia service theo ranh giới trách nhiệm rõ ràng:

- `control-api/`: ASP.NET Core service cho control plane.
- `ingestion-api/`: ASP.NET Core service xác thực API key, áp dụng idempotency/rate limit và publish log vào Kafka.
- `log-processor/`: .NET Worker consume Kafka, xử lý batch, route DLQ và index vào OpenSearch.

Trong Docker Compose, Nginx là public entry point; các application service chỉ dùng mạng nội bộ. Mỗi service có README riêng để mô tả trách nhiệm, dependency và phạm vi không được vượt qua.
