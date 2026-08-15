# Health Checks

Health check giúp biết service có sẵn sàng nhận traffic hay không.

## Loại health check

- Liveness: process còn sống.
- Readiness: service có thể phục vụ request.
- Dependency health: PostgreSQL, Kafka, OpenSearch, Redis.

## Nguyên tắc

Readiness phải fail nếu dependency bắt buộc không sẵn sàng. Liveness không nên phụ thuộc quá nhiều vào network để tránh restart loop không cần thiết.
