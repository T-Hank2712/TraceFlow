# Security Checklist

## Trước khi merge

- Không có secret trong code, docs hoặc log mẫu.
- API key không lưu plaintext.
- Endpoint project/search/dashboard đều kiểm tra authorization.
- Ingestion không tin `project_id` từ client.
- Error response không lộ stack trace.
- Request size limit đã được cấu hình.

## Trước khi release

- JWT secret đủ mạnh.
- Database migration đã kiểm tra constraint.
- OpenSearch query có tenant filter.
- Rate limiting được bật cho ingestion.
- Audit log cho thao tác nhạy cảm.
