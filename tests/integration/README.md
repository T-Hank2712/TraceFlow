# Integration Tests

Thư mục này chứa integration tests kiểm tra tương tác giữa service và dependency thật hoặc containerized.

Các dependency chính cần kiểm tra gồm PostgreSQL, Kafka, OpenSearch và Redis nếu được sử dụng.

Integration tests phải reset dữ liệu giữa các lần chạy để kết quả có thể lặp lại.
