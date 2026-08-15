# Contract Tests

Contract test bảo vệ contract giữa các service.

## Contract cần bảo vệ

- Management API request/response.
- Ingestion API request/response.
- Search API request/response.
- Kafka log event schema.
- OpenSearch document schema.

## Nguyên tắc

Khi schema thay đổi, test phải chỉ ra service nào bị ảnh hưởng.
