# System Design Notes

TraceFlow là ví dụ về kiến trúc ingestion pipeline có buffer.

## Chủ đề cần nắm

- Data plane và control plane.
- At-least-once delivery.
- Backpressure.
- Batch processing.
- Multi-tenancy.
- Search indexing.
- Failure isolation.

## Câu hỏi thiết kế

- Khi OpenSearch chậm thì ingestion có bị ảnh hưởng không?
- Khi consumer crash thì log có mất không?
- Làm sao tránh user đọc log của project khác?
- Batch size ảnh hưởng throughput và latency thế nào?
