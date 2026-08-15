# Go Ingestion Service

Go Ingestion Service nhận log từ application và publish vào Kafka.

## Trách nhiệm

- Nhận single log và batch logs.
- Xác thực API key.
- Validate payload.
- Chuẩn hóa log event.
- Publish vào Kafka.
- Rate limiting và request size limit.
- Structured logging và metrics.

## Endpoint

- `POST /v1/logs`
- `POST /v1/logs/batch`

## Nguyên tắc

Service trả thành công chỉ khi event đã được publish vào Kafka theo policy. Không được silently discard log.
