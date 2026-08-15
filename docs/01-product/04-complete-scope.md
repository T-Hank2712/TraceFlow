# Phạm Vi Sản Phẩm Hoàn Chỉnh

Tài liệu này mô tả phạm vi đích của TraceFlow khi hoàn thiện, không giới hạn theo từng giai đoạn triển khai.

## Năng lực sản phẩm

- Authentication và quản lý phiên đăng nhập.
- Workspace, member và vai trò trong workspace.
- Project, application/service và API key.
- Single log ingestion và batch log ingestion.
- Kafka-based asynchronous pipeline.
- Go processor với batching, retry, DLQ và graceful shutdown.
- OpenSearch indexing, full-text search và filter nâng cao.
- Trace search theo `trace_id` và `correlation_id`.
- Dashboard metrics cho log volume, error rate, service health và top errors.
- Alerting rule và notification channel.
- Retention policy theo project/plan.
- Usage tracking và quota enforcement.
- Rate limiting cho ingestion và user API.
- Observability cho chính TraceFlow: logs, metrics, health checks và tracing nội bộ.
- Docker-based local development và tài liệu vận hành.
- Test đầy đủ: unit, integration, contract, end-to-end và performance.

## Ranh giới sản phẩm

TraceFlow không nhằm thay thế hoàn toàn các nền tảng thương mại như Datadog, Splunk, Elastic hay Grafana. Mục tiêu là xây dựng một project backend có chiều sâu, có business flow rõ ràng và đủ thực tế để thảo luận về system design, reliability và observability.

## Tiêu chí hoàn thiện

TraceFlow được xem là hoàn thiện khi một đội kỹ thuật có thể:

- Tạo workspace/project/application.
- Cấp và thu hồi API key.
- Gửi log từ nhiều service.
- Tìm kiếm log theo text, filter và time range.
- Truy vết request xuyên service bằng `trace_id`.
- Xem dashboard lỗi và lưu lượng.
- Cấu hình alert rule.
- Áp dụng retention policy.
- Theo dõi usage/quota.
- Vận hành local stack ổn định bằng Docker Compose.
- Chạy test và benchmark có thể lặp lại.
