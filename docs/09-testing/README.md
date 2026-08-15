# Testing

Nhóm tài liệu này mô tả chiến lược test của TraceFlow. Vì TraceFlow có pipeline bất đồng bộ, multi-tenancy và nhiều storage khác nhau, chỉ unit test là không đủ. Hệ thống cần test ở nhiều tầng để bảo vệ correctness, contract và reliability.

Testing docs giúp trả lời: logic nào test bằng unit test, dependency nào cần integration test, flow nào cần end-to-end test, contract nào cần khóa lại, và benchmark nào dùng để đo throughput/latency.

## Nội dung cần nắm

- Unit test dùng cho logic nhỏ như validation, hashing, query builder và alert evaluation.
- Integration test kiểm tra PostgreSQL, Kafka, OpenSearch và repository/client thật.
- End-to-end test kiểm tra log đi từ ingestion tới search.
- Contract test bảo vệ API schema, Kafka event schema và OpenSearch document schema.
- Performance test đo ingestion latency, processing throughput, Kafka lag và OpenSearch indexing latency.
- Test data phải có multi-tenant case để tránh lộ dữ liệu.

## Khi nào cần cập nhật nhóm này

- Khi thêm domain rule hoặc validator mới.
- Khi thêm endpoint hoặc event schema mới.
- Khi đổi storage/query behavior.
- Khi thêm failure policy như retry, DLQ hoặc rate limiting.
- Khi có benchmark mới hoặc phát hiện bottleneck.

## Thứ tự đọc

1. `01-strategy.md`
2. `02-unit-tests.md`
3. `03-integration-tests.md`
4. `04-end-to-end-tests.md`
5. `05-contract-tests.md`
6. `06-performance-tests.md`
7. `07-test-data.md`

## Kết quả mong đợi sau khi đọc

Sau khi đọc xong nhóm này, người đọc phải biết thay đổi nào cần loại test nào và cách xác minh flow quan trọng nhất: gửi log, xử lý qua Kafka, index OpenSearch và search lại được.
