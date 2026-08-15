# Observability Của TraceFlow

TraceFlow tự nó cũng cần có logging, metrics và health check.

## Structured logging

Mọi service nên log dạng structured với các trường:

- `service`
- `environment`
- `request_id`
- `trace_id` nếu có
- `error`
- `duration_ms`

## Metrics cần có

- Ingestion request count.
- Ingestion error count.
- Kafka publish latency.
- Consumer lag.
- Processor batch size.
- OpenSearch indexing latency.
- Search request latency.

## Health checks

- ASP.NET Core API kiểm tra PostgreSQL và OpenSearch nếu cần.
- Go Ingestion kiểm tra Kafka và kết nối metadata store/API key store.
- Processor kiểm tra Kafka consumer và OpenSearch.
