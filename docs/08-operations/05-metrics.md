# Metrics

Metrics giúp vận hành và benchmark TraceFlow. Trong core scope, metrics chỉ cần đủ để debug ingestion, processing, Redis fallback, OpenSearch indexing, search và DLQ.

## Metrics chính

- Ingestion requests.
- Ingestion latency p50/p95/p99.
- Kafka publish latency.
- Kafka consumer lag.
- Processor throughput.
- Batch size.
- OpenSearch indexing latency.
- Search latency.
- DLQ event count.
- Redis fallback count.
- Redis rate limit decisions.

## Nguyên tắc

Metrics phải có label vừa đủ. Tránh label có cardinality quá cao như raw `trace_id`. Alert evaluation metrics chỉ cần nếu optional alerting được triển khai.
