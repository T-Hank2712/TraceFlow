# Data Plane

Data plane là phần xử lý luồng log tốc độ cao.

## Thành phần

- Go Ingestion Service.
- Kafka topic chứa log event.
- Go Log Processor.
- OpenSearch index.

## Luồng xử lý

```text
Client App
-> HTTP Ingestion API
-> Validate API Key
-> Normalize Log
-> Kafka
-> Processor Consumer Group
-> Batch
-> OpenSearch Bulk Index
```

## Nguyên tắc

- Không để OpenSearch chậm làm block trực tiếp ingestion request.
- Không giữ queue trong memory không giới hạn.
- Mọi lỗi publish, consume hoặc index phải có policy rõ.
- Mỗi log phải mang `project_id` và `application_id` sau khi xác thực API key.
