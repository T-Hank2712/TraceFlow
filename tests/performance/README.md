# Performance Tests

Thư mục này chứa performance tests và benchmark cho TraceFlow.

Các chỉ số cần đo gồm logs per second, ingestion latency p50/p95/p99, Kafka consumer lag, processor throughput, OpenSearch bulk indexing latency, CPU và memory.

Benchmark dùng để ra quyết định tối ưu dựa trên số liệu, không tối ưu theo cảm tính.

## Kịch bản hiện có

- `k6/run/baseline.sh`, `single-log.sh`, `batch-log.sh`, `load.sh`, `spike.sh`, `stress.sh` và `capacity.sh` đo HTTP ingestion dưới các kiểu tải khác nhau.
- `k6/run/e2e.sh` gửi marker để truy vết thủ công.
- `k6/run/e2e-reconciliation.sh` gửi 50.000 event có định danh riêng và chạy verifier đối soát OpenSearch/DLQ; đây là kiểm tra end-to-end correctness, không chỉ là benchmark.

Xem hướng dẫn chạy stack, health check, log và reset tại `tests/performance/TESTING.md`.
