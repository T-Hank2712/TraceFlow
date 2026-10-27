# OpenSearch Indexes

OpenSearch lưu log phục vụ search. Operational insights có thể đọc aggregation từ OpenSearch nếu nice-to-have này được bật.

## Trường index chính

- `timestamp`
- `workspaceId`
- `projectId`
- `applicationId`
- `service`
- `environment`
- `level`
- `message`
- `traceId`
- `correlationId`
- `metadata`

## Mapping nguyên tắc

- Control API hiện chỉ filter/sort; chưa gửi full-text query cho `message`.
- Các field filter như `workspaceId`, `projectId`, `applicationId`, `level` và `service` cần query được dạng keyword.
- `timestamp` dùng date cho range query và sort.
- Processor hiện index `LogEvent`, không giữ `receivedAt` hoặc `processedAt` trong document.
- `metadata` cần thiết kế cẩn thận để tránh mapping explosion.
