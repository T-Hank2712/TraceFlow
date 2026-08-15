# Testing Strategy

TraceFlow cần test ở nhiều tầng vì hệ thống có cả API nghiệp vụ và pipeline bất đồng bộ.

## Tầng test

- Unit test cho domain logic và validator.
- Integration test với PostgreSQL, Kafka và OpenSearch.
- Contract test cho API và Kafka event schema.
- End-to-end test cho log flow.
- Performance test cho ingestion và processing.

## Nguyên tắc

Test phải bảo vệ correctness, tenant isolation và failure behavior trước khi tối ưu performance.
