# Metrics

Metrics giúp vận hành và benchmark TraceFlow.

## Metrics chính

- Ingestion requests.
- Ingestion latency p50/p95/p99.
- Kafka publish latency.
- Kafka consumer lag.
- Processor throughput.
- Batch size.
- OpenSearch indexing latency.
- Search latency.
- Alert evaluation latency.
- DLQ event count.

## Nguyên tắc

Metrics phải có label vừa đủ. Tránh label có cardinality quá cao như raw `trace_id`.
