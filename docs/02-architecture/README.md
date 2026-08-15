# Architecture

Nhóm tài liệu này mô tả kiến trúc hệ thống của TraceFlow. Đây là nơi chốt các ranh giới quan trọng: service nào chịu trách nhiệm phần nào, dữ liệu log đi qua pipeline ra sao, control plane và data plane tách nhau thế nào, và hệ thống phản ứng thế nào khi dependency lỗi hoặc downstream chậm.

TraceFlow có nhiều thành phần: ASP.NET Core Control API, Go Ingestion Service, Kafka, Go Log Processor, PostgreSQL và OpenSearch. Nếu kiến trúc không rõ, project rất dễ bị trộn trách nhiệm, ví dụ ingestion index thẳng vào OpenSearch, search bỏ qua tenant filter hoặc processor commit offset sai thời điểm.

## Nội dung cần nắm

- Control plane quản lý user, workspace, project, application, API key, search, dashboard và cấu hình.
- Data plane nhận log tốc độ cao, publish Kafka, process batch và index OpenSearch.
- Kafka là ranh giới bất đồng bộ giữa ingestion và processing.
- OpenSearch là nơi lưu log phục vụ search, trace lookup và dashboard aggregation.
- Multi-tenancy là yêu cầu kiến trúc, không phải chi tiết phụ.
- Failure handling, backpressure và observability phải được thiết kế từ đầu.

## Khi nào cần cập nhật nhóm này

- Khi thêm service mới hoặc thay đổi ranh giới service.
- Khi thay đổi pipeline ingestion/processing/search.
- Khi thêm retry, DLQ, rate limiting, retention hoặc alert worker.
- Khi thay đổi storage responsibility giữa PostgreSQL, Kafka, OpenSearch hoặc Redis.
- Khi có quyết định kỹ thuật đủ lớn để cần ADR.

## Thứ tự đọc

1. `01-system-overview.md`
2. `02-service-boundaries.md`
3. `03-data-plane.md`
4. `04-control-plane.md`
5. `05-ingestion-pipeline.md`
6. `06-processing-pipeline.md`
7. `07-search-architecture.md`
8. `08-multi-tenancy.md`
9. `09-failure-handling.md`
10. `10-backpressure.md`
11. `11-observability.md`
12. `12-architecture-decisions/README.md`

## Kết quả mong đợi sau khi đọc

Sau khi đọc xong nhóm này, người đọc phải vẽ lại được sơ đồ TraceFlow, giải thích vì sao Go và ASP.NET Core được dùng ở các phần khác nhau, và chỉ ra dữ liệu log đi từ application tới OpenSearch theo những bước nào.
