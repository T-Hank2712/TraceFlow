# Ranh Giới Service

## ASP.NET Core Control API

Chịu trách nhiệm:

- Authentication.
- Workspace, Project, Application.
- API Key management.
- Search API.
- Internal API key validation.
- Nice-to-have: retention/quota/operational insights ở mức nhỏ nếu được bật.

Không chịu trách nhiệm nhận log tốc độ cao trực tiếp. Data plane luôn đi qua Ingestion API.

## ASP.NET Core Ingestion API

Chịu trách nhiệm:

- Nhận single/batch log.
- Xác thực API key.
- Validate và chuẩn hóa payload.
- Publish event vào Kafka.
- Dùng Redis cho validation cache, idempotency và rate limit.
- Resolve client IP từ trusted proxy trước pre-auth rate limit.
- Trả `200 OK` sau Kafka acknowledgement, hoặc `503` khi delivery không thể xác nhận.

Không index trực tiếp vào OpenSearch trong HTTP request.

## .NET Log Processor

Chịu trách nhiệm:

- Consume Kafka.
- Normalize/enrich log.
- Gom batch.
- Bulk index vào OpenSearch.
- Xử lý retry, timeout, DLQ và commit offset theo kết quả xử lý.

## Storage Boundary

- PostgreSQL lưu metadata nghiệp vụ.
- Redis lưu cache/counter ngắn hạn, không phải source of truth.
- Kafka lưu log stream tạm thời.
- OpenSearch lưu log phục vụ tìm kiếm.
