# Deployments

Thư mục này chứa cấu hình triển khai và vận hành TraceFlow.

Ở giai đoạn local development, cấu hình chính nằm trong `docker-compose/` để chạy PostgreSQL, Kafka, OpenSearch, Redis và các service ứng dụng khi được scaffold.

Các cấu hình production hoặc environment-specific có thể được bổ sung sau nhưng phải giữ rõ ranh giới giữa local, staging và production.
