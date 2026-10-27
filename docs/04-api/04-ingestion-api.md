# Ingestion API

Ingestion API do ASP.NET Core Ingestion API cung cấp.

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
POST /v1/batch-logs
```

Nhận danh sách log. Batch có giới hạn số lượng và dung lượng; từng item được validate/publish độc lập và response trả accepted/rejected theo item.

## Nguyên tắc

Client không được tự quyết định `workspaceId`, `projectId`, `applicationId` hoặc `environment`. Các trường tenant này được suy ra từ API key sau khi Control API validate.
