# Performance Benchmark Notes

Benchmark dùng để quyết định tối ưu dựa trên số liệu.

## Chỉ số

- Logs per second.
- Ingestion latency p50/p95/p99.
- Processing throughput.
- Consumer lag.
- OpenSearch indexing latency.
- CPU.
- Memory.

## Nguyên tắc

Không đặt mục tiêu throughput tùy tiện. Đầu tiên đo baseline, sau đó thay đổi từng biến như batch size, số consumer, partition count hoặc OpenSearch setting.
