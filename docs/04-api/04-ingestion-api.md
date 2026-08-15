# Ingestion API

Ingestion API do Go Ingestion Service cung cấp.

## Authentication

Client gửi API key qua header:

```text
Authorization: ApiKey <secret>
```

## Single log

```text
POST /v1/logs
```

Nhận một log object và trả `202 Accepted` nếu đã publish vào Kafka thành công.

## Batch logs

```text
POST /v1/logs/batch
```

Nhận danh sách log. Batch phải có giới hạn số lượng và dung lượng.

## Nguyên tắc

Client không được tự quyết định `project_id` hoặc `application_id`. Hai trường này được suy ra từ API key.
