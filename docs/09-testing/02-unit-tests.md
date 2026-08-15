# Unit Tests

Unit test kiểm tra logic nhỏ, nhanh và không phụ thuộc external service.

## Nên test

- Validation log payload.
- API key hashing/verification.
- Authorization policy.
- Search query builder.
- Retention calculation.
- Alert rule evaluation logic.

## Nguyên tắc

Unit test không dùng PostgreSQL, Kafka hoặc OpenSearch thật.
