# Integration Tests

Integration test kiểm tra tương tác với dependency thật hoặc containerized.

## Nên test

- PostgreSQL repository và migration.
- Kafka producer/consumer.
- OpenSearch indexing và query.
- API key lookup.
- Search API với tenant filter.

## Nguyên tắc

Integration test phải reset dữ liệu giữa các test để kết quả có thể lặp lại.
