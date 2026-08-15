# OpenSearch Indexes

OpenSearch lưu log phục vụ search và dashboard.

## Trường index chính

- `timestamp`
- `project_id`
- `application_id`
- `service`
- `environment`
- `level`
- `message`
- `trace_id`
- `correlation_id`
- `metadata`
- `received_at`
- `processed_at`

## Mapping nguyên tắc

- `message` hỗ trợ full-text search.
- Các field filter như `project_id`, `level`, `service` dùng keyword.
- `timestamp`, `received_at`, `processed_at` dùng date.
- `metadata` cần thiết kế cẩn thận để tránh mapping explosion.
