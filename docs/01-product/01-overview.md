# Tổng Quan Sản Phẩm

TraceFlow là một nền tảng backend giúp thu thập log từ nhiều ứng dụng hoặc service, xử lý bất đồng bộ, index vào search engine và cho phép người dùng tìm kiếm, lọc, truy vết log theo thời gian.

## Mục tiêu

- Tập trung log từ nhiều service vào một nơi.
- Hỗ trợ tìm kiếm theo `service`, `environment`, `level`, `message`, `trace_id`, `correlation_id`, `timestamp` và `metadata`.
- Giúp Backend Developer, DevOps và SRE điều tra sự cố nhanh hơn.
- Thực hành kiến trúc backend có chiều sâu: Kafka, OpenSearch, batch processing, backpressure, retry và observability.

## Không phải mục tiêu sản phẩm

- Không cạnh tranh với Datadog, Splunk hoặc Grafana.
- Không biến project thành tập hợp công nghệ không có use case rõ.
- Không ưu tiên tính năng trình diễn hơn correctness, bảo mật và khả năng vận hành.
