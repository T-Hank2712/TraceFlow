# Tests

Thư mục này chứa các test cấp hệ thống của TraceFlow.

Test riêng cho từng service có thể nằm trong `services/*/tests`, còn thư mục này tập trung vào các bài test kiểm tra nhiều thành phần cùng lúc.

Các nhóm test chính:

- `e2e/`: kiểm tra luồng từ tạo project/application/API key tới gửi log và search log.
- `integration/`: kiểm tra tích hợp với PostgreSQL, Kafka, OpenSearch và các service.
- `performance/`: đo ingestion latency, throughput, Kafka lag và indexing performance.
