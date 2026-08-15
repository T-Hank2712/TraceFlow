# Docker Compose

Thư mục này chứa Docker Compose stack cho môi trường phát triển local.

Local stack dùng để chạy các dependency chính của TraceFlow như PostgreSQL, Kafka, OpenSearch và Redis. Khi các service ứng dụng được scaffold, chúng có thể được thêm vào compose để kiểm tra end-to-end flow.

File cấu hình trong thư mục này không được chứa secret thật. Secret local nên được lấy từ `.env` dựa trên `.env.example`.
