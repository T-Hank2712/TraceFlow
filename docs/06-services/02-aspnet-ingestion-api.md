# ASP.NET Core Ingestion API

ASP.NET Core Ingestion API nhận log từ application và publish vào Kafka.

## Trách nhiệm

- Nhận single log và batch logs.
- Xác thực API key.
- Kiểm tra marker revoke trước khi dùng tenant context cache.
- Validate payload.
- Chuẩn hóa log event.
- Cấp hoặc dùng lại `eventId` bằng Redis-backed idempotency record.
- Publish vào Kafka.
- Rate limiting theo client IP đã resolve từ trusted proxy, và request size limit.
- Readiness kiểm tra Redis, Kafka topic và Control API validation endpoint.
- Structured logging và metrics.

## Endpoint

- `POST /v1/logs`
- `POST /v1/batch-logs`

## Nguyên tắc

Service trả `200 OK` chỉ khi Kafka acknowledge event theo publish policy. Không được silently discard log hoặc báo thành công khi Kafka publish thất bại.

Mọi request ghi log yêu cầu header `Idempotency-Key` không rỗng, tối đa 128 ký tự. Retry phải gửi cùng key và payload trong thời gian TTL 24 giờ; dùng lại key với payload khác trả `409 Conflict`. Redis idempotency không sẵn sàng trả `503 Service Unavailable` thay vì sinh event ID mới.

Single và batch request có Kafka delivery failure trả `503 Service Unavailable`; batch response giữ kết quả theo từng item để client phân biệt item được Kafka xác nhận với item không được xác nhận.
