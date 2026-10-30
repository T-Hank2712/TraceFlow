# Performance Tests

Performance test dùng để đo throughput và latency, không dùng để đoán trước tối ưu.

## Chỉ số

- Logs per second.
- Ingestion p50/p95/p99.
- Kafka publish latency.
- Kafka consumer lag.
- Processor throughput.
- OpenSearch bulk latency.
- CPU và memory.

## Kịch bản

- Single log high concurrency.
- Batch log ingestion.
- OpenSearch chậm.
- Processor scale nhiều instance.
- Spike traffic.

## Diễn giải kết quả

- `http_req_duration` đo thời gian Ingestion API phản hồi sau khi Kafka acknowledge, không phải thời gian log xuất hiện ở OpenSearch.
- Phải theo dõi Kafka consumer lag, Log Processor và OpenSearch cùng lúc với k6; p95 xấu hoặc request reset có thể đến từ backlog/consumer/indexing, không chỉ từ load generator.
- E2E reconciliation là kiểm tra correctness của pipeline sau load, khác với capacity/soak test chỉ đo HTTP throughput và latency.
- Chỉ so sánh các lần chạy trên pipeline sạch hoặc có ghi rõ trạng thái Kafka/OpenSearch, vì backlog cũ làm kết quả latency và E2E khác nhau.
