# Unit Tests

Unit test kiểm tra logic nhỏ, nhanh và không phụ thuộc external service.

## Nên test

- Validation log payload.
- API key hashing/verification.
- Authorization policy.
- Search query builder.
- Retention calculation nếu triển khai nice-to-have retention.
- Alert rule evaluation logic chỉ cần nếu optional alerting được triển khai.

## Nguyên tắc

Unit test không dùng PostgreSQL, Kafka hoặc OpenSearch thật.
