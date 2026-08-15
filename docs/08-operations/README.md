# Operations

Nhóm tài liệu này mô tả cách cấu hình, chạy, quan sát, backup và debug TraceFlow. Đây là phần biến project từ “code có thể compile” thành “hệ thống có thể vận hành”.

TraceFlow phụ thuộc nhiều thành phần hạ tầng như PostgreSQL, Kafka, OpenSearch và có thể có Redis. Vì vậy operations docs phải giúp người phát triển hiểu cách service khởi động, dependency nào cần health check, cấu hình nào là bắt buộc, log/metrics nào cần nhìn khi lỗi xảy ra.

## Nội dung cần nắm

- Docker Compose chạy local stack đầy đủ.
- Configuration qua environment variables và `.env.example`.
- Migration quản lý schema PostgreSQL.
- Logging và metrics giúp debug ingestion, processing, search và alerting.
- Health checks phân biệt liveness, readiness và dependency health.
- Backup/restore bảo vệ metadata và cấu hình quan trọng.
- Troubleshooting mô tả cách điều tra lỗi thường gặp.

## Khi nào cần cập nhật nhóm này

- Khi thêm service hoặc dependency mới vào local stack.
- Khi thêm biến môi trường mới.
- Khi thay đổi migration strategy hoặc storage.
- Khi thêm metrics, log field hoặc health endpoint.
- Khi gặp lỗi vận hành lặp lại và cần ghi cách xử lý.

## Thứ tự đọc

1. `01-docker-compose.md`
2. `02-configuration.md`
3. `03-migrations.md`
4. `04-logging.md`
5. `05-metrics.md`
6. `06-health-checks.md`
7. `07-backup-and-restore.md`
8. `08-troubleshooting.md`

## Kết quả mong đợi sau khi đọc

Sau khi đọc xong nhóm này, người đọc phải có thể chạy local stack, biết service nào đang lỗi, xem log/metrics phù hợp và điều tra các lỗi như Kafka lag, OpenSearch chậm hoặc search không thấy log.
