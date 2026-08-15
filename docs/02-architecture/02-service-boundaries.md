# Ranh Giới Service

## ASP.NET Core Control API

Chịu trách nhiệm:

- Authentication.
- Workspace, Project, Application.
- API Key management.
- Search API.
- Dashboard API.
- Alert configuration.
- Retention configuration.
- Usage and quota configuration.

Không chịu trách nhiệm nhận log tốc độ cao trực tiếp. Data plane luôn đi qua Go Ingestion Service.

## Go Ingestion Service

Chịu trách nhiệm:

- Nhận single/batch log.
- Xác thực API key.
- Validate và chuẩn hóa payload.
- Publish event vào Kafka.
- Trả `202 Accepted` nhanh cho client.

Không index trực tiếp vào OpenSearch trong HTTP request.

## Go Log Processor

Chịu trách nhiệm:

- Consume Kafka.
- Normalize/enrich log.
- Gom batch.
- Index vào OpenSearch.
- Xử lý retry, timeout và graceful shutdown.

## Storage Boundary

- PostgreSQL lưu metadata nghiệp vụ.
- Kafka lưu log stream tạm thời.
- OpenSearch lưu log phục vụ tìm kiếm.
