# Kiến Trúc Tổng Thể

TraceFlow được chia thành hai mặt phẳng chính:

- Control plane: quản lý user, workspace, project, application, API key, search và dashboard API.
- Data plane: nhận log tốc độ cao, đẩy vào Kafka, xử lý batch và index vào OpenSearch.

```text
Applications
    -> Go Ingestion API
    -> Kafka
    -> Go Log Processor
    -> OpenSearch
    <- ASP.NET Core Search API
```

## Thành phần chính

- ASP.NET Core API: control plane và search API.
- Go Ingestion Service: nhận log từ application.
- Kafka: buffer và stream log event.
- Go Log Processor: consume, batch, enrich và index log.
- PostgreSQL: lưu dữ liệu nghiệp vụ.
- OpenSearch: lưu và tìm kiếm log.

## Nguyên tắc thiết kế

Không thêm service mới nếu chưa có ranh giới trách nhiệm rõ ràng. Ưu tiên correctness, observability và luồng end-to-end chạy được trước khi tối ưu throughput.
