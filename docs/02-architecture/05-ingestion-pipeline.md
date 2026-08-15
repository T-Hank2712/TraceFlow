# Ingestion Pipeline

Ingestion pipeline nhận log từ application và đưa vào Kafka.

## Endpoint

- `POST /v1/logs`
- `POST /v1/logs/batch`

## Các bước xử lý

1. Đọc API key từ header.
2. Hash/lookup API key để xác định project và application.
3. Validate payload.
4. Chuẩn hóa timestamp, level, service, environment và metadata.
5. Gắn `project_id`, `application_id`, `received_at`.
6. Publish vào Kafka.
7. Trả `202 Accepted`.

## Nguyên tắc

Ingestion service không thực hiện OpenSearch indexing trực tiếp. Nếu Kafka unavailable, service phải trả lỗi rõ ràng thay vì giả vờ nhận log thành công.
