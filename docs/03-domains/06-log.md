# Domain: Log

Log là dữ liệu chính được TraceFlow thu thập và tìm kiếm.

## Trường cơ bản

- `timestamp`
- `level`
- `workspaceId`
- `projectId`
- `applicationId`
- `service`
- `environment`
- `message`
- `trace_id`
- `correlation_id`
- `metadata`
- `receivedAt` trong Kafka event; processor hiện không lưu field này vào OpenSearch.

## Log levels

TraceFlow hỗ trợ: `TRACE`, `DEBUG`, `INFO`, `WARN`, `ERROR`, `FATAL`.

## Nguyên tắc

Log sử dụng structured logging. `metadata` là key-value mở rộng, không ép tất cả log theo cùng schema nghiệp vụ.
