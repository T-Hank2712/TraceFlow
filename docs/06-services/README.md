# Services

Nhóm tài liệu này mô tả các service chính trong TraceFlow và trách nhiệm của từng service. Đây là cầu nối giữa kiến trúc tổng thể và implementation thực tế.

Service docs phải làm rõ mỗi service sở hữu endpoint/job nào, dependency nào là bắt buộc, dữ liệu nào được đọc/ghi và ranh giới nào không được vượt qua. Ví dụ Go Ingestion Service được phép validate API key và publish Kafka, nhưng không được tự ý trở thành service quản lý workspace.

## Nội dung cần nắm

- ASP.NET Core Control API sở hữu control plane, search API, dashboard API và cấu hình sản phẩm.
- Go Ingestion Service sở hữu HTTP ingestion, API key validation, payload normalization và Kafka publishing.
- Go Log Processor sở hữu Kafka consumption, batch processing, retry, DLQ và OpenSearch indexing.
- Local Development Stack mô tả cách các service chạy cùng nhau trong môi trường phát triển.

## Khi nào cần cập nhật nhóm này

- Khi thêm endpoint, worker hoặc background job vào service.
- Khi service thêm dependency mới như Redis, SMTP, notification provider.
- Khi thay đổi ownership dữ liệu hoặc ranh giới service.
- Khi thay đổi cách chạy local stack hoặc health check.

## Thứ tự đọc

1. `01-aspnet-control-api.md`
2. `02-go-ingestion-service.md`
3. `03-go-log-processor.md`
4. `04-local-development-stack.md`

## Kết quả mong đợi sau khi đọc

Sau khi đọc xong nhóm này, người đọc phải biết code cho một tính năng nên nằm ở service nào và service đó cần giao tiếp với dependency nào.
