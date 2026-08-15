# Contracts

Thư mục này chứa các contract dùng chung giữa các service của TraceFlow.

Contract giúp các service phát triển độc lập nhưng vẫn thống nhất về request/response, event schema và search document format.

Các nhóm contract chính:

- `openapi/`: API contract cho Management, Ingestion và Search APIs.
- `kafka/`: schema cho event được publish qua Kafka.
- `opensearch/`: mapping và cấu trúc document trong OpenSearch.
