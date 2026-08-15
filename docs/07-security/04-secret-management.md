# Secret Management

Secret gồm database password, JWT signing key, API key hash pepper, Kafka credential, OpenSearch credential và notification token.

## Nguyên tắc

- Không commit secret vào repo.
- Dùng `.env.example` cho biến môi trường mẫu.
- Secret thật lấy từ environment hoặc secret manager.
- Log không được in secret.
- Test không phụ thuộc secret production.

## Local development

Local stack có thể dùng secret giả, nhưng phải tách rõ khỏi cấu hình production.
