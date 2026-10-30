# Ingestion Pipeline

Ingestion pipeline nhận log từ application và đưa vào Kafka.

## Endpoint

- `POST /v1/logs`
- `POST /v1/batch-logs`

## Các bước xử lý

1. Đọc API key từ header.
2. Hash/lookup API key để xác định project và application.
3. Validate payload.
4. Chuẩn hóa timestamp, level, service, environment và metadata.
5. Dùng `Idempotency-Key` để lấy hoặc tạo `eventId`, rồi gắn `workspaceId`, `projectId`, `applicationId`, `environment`, `batchId` và `receivedAt`.
6. Publish vào Kafka.
7. Trả `200 OK` sau khi Kafka acknowledge; trả `503` nếu publish không thể được xác nhận.

## Nguyên tắc

Ingestion service không thực hiện OpenSearch indexing trực tiếp. Nếu Kafka unavailable, service phải trả lỗi rõ ràng thay vì giả vờ nhận log thành công.
