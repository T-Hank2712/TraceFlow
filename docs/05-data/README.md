# Data

Nhóm tài liệu này mô tả thiết kế dữ liệu của TraceFlow. Đây là nơi chốt dữ liệu nào nằm ở PostgreSQL, dữ liệu nào nằm ở Kafka, dữ liệu nào nằm ở OpenSearch và contract giữa các storage đó là gì.

TraceFlow có nhiều loại dữ liệu với đặc tính khác nhau. User, workspace, project và API key là dữ liệu transaction nên thuộc PostgreSQL. Log là dữ liệu khối lượng lớn, cần full-text search và time range query nên thuộc OpenSearch. Kafka giữ log event như stream trung gian giữa ingestion và processing.

## Nội dung cần nắm

- PostgreSQL bảo vệ invariant nghiệp vụ bằng foreign key, unique constraint và transaction.
- OpenSearch phục vụ search, trace lookup và dashboard aggregation.
- Kafka topic là contract bất đồng bộ giữa producer và consumer.
- Log event schema phải đủ thông tin để processor index mà không phải tin lại client.
- API key storage phải tránh lưu plaintext secret.
- Retention policy quyết định vòng đời log.
- Sample data giúp test và demo luồng end-to-end.

## Khi nào cần cập nhật nhóm này

- Khi thêm bảng, index, topic hoặc event schema mới.
- Khi đổi mapping OpenSearch hoặc query pattern.
- Khi đổi cách hash/lưu API key.
- Khi thêm retention, quota hoặc usage counter.
- Khi thay đổi schema ảnh hưởng API hoặc service boundary.

## Thứ tự đọc

1. `01-postgres-schema.md`
2. `02-opensearch-indexes.md`
3. `03-kafka-topics.md`
4. `04-log-event-schema.md`
5. `05-api-key-storage.md`
6. `06-retention-policy.md`
7. `07-sample-data.md`

## Kết quả mong đợi sau khi đọc

Sau khi đọc xong nhóm này, người đọc phải biết một field nên được lưu ở đâu, vì sao lưu ở đó và service nào chịu trách nhiệm ghi/đọc nó.
