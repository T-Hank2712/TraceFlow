# Deployments

Thư mục này chứa cấu hình triển khai và vận hành TraceFlow.

- `../docker-compose.yml`: local stack đầy đủ; Nginx expose `80` và `443`.
- `performance/docker-compose.performance.yml`: performance stack; Nginx expose `8080` và `8443`, kèm load generator, network simulator và k6.
- `nginx/nginx.conf`: route public `/control/` tới Control API và `/ingestion/` tới Ingestion API; internal Control API routes không được expose qua Nginx.

Kafka, OpenSearch, Redis và các application service giao tiếp trên mạng Docker nội bộ. Mỗi Compose file có named volumes riêng; chỉ dùng `down -v` khi chủ động reset dữ liệu của stack tương ứng.
