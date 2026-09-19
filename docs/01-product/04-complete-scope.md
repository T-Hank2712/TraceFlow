# Phạm Vi Sản Phẩm

Tài liệu này chốt phạm vi sản phẩm theo ba tầng: Core, Nice-to-have và Optional. Mục tiêu là giữ TraceFlow đủ sâu để thể hiện năng lực backend, nhưng không phình thành một observability platform quá lớn.

## Core Scope

TraceFlow Core là:

```text
Multi-tenant log ingestion and search pipeline
```

Core bao gồm các năng lực bắt buộc:

| Capability | Ý nghĩa |
| :--- | :--- |
| Auth/RBAC | User authentication và kiểm soát quyền theo workspace/project. |
| Workspace/Project/Application | Resource hierarchy để xác định tenant boundary và log ownership. |
| API Key | Credential riêng cho client application gửi log. |
| Ingestion API | Nhận single log và batch logs. |
| Kafka | Buffer và event stream giữa ingestion và processing. |
| Log Processor | Consume Kafka, validate, batch và index log. |
| OpenSearch | Search storage cho log document. |
| Batch + Bulk Indexing | Tăng throughput và giảm per-message indexing. |
| Retry + DLQ | Xử lý invalid event và downstream failure. |
| Search API | User search log qua Control API với project scope. |
| Redis | Cache, rate limit và counter ngắn hạn; không phải source of truth. |

## Nice-To-Have Scope

Các phần này có giá trị nhưng chỉ nên làm sau khi core ổn định.

| Capability | Guardrail |
| :--- | :--- |
| Retention | Fixed policy như 7/14/30/90 ngày, không archive/restore. |
| Quota | Bảo vệ ingestion path, không làm billing. |
| Operational Insights | Summary API cơ bản như volume, error count, error rate, top services. |
| Benchmark | Đo throughput logs/s, ingestion latency và searchable latency. |
| Minimal Web UI | Demo core flow, không xây advanced dashboard. |

## Optional Scope

| Capability | Lý do optional |
| :--- | :--- |
| Alerting | Dễ kéo scheduler, rule engine và notification workflow. |
| Backup/Restore | Hữu ích nhưng không nằm trong core pipeline. |
| Advanced Dashboard | Dễ làm project lệch sang frontend/analytics. |

## Ranh giới sản phẩm

TraceFlow không nhằm thay thế hoàn toàn các nền tảng thương mại như Datadog, Splunk, Elastic hay Grafana. Mục tiêu là xây dựng một project backend có chiều sâu, có business flow rõ ràng và đủ thực tế để thảo luận về system design, reliability và observability.

## Tiêu chí hoàn thiện

TraceFlow Core được xem là hoàn thiện khi một đội kỹ thuật có thể:

- Tạo workspace/project/application.
- Cấp và thu hồi API key.
- Gửi log từ nhiều service.
- Tìm kiếm log theo text, filter và time range.
- Truy vết request xuyên service bằng `trace_id`.
- Chứng minh Kafka decouple ingestion khỏi indexing.
- Chứng minh batch/bulk indexing, retry và DLQ hoạt động đúng.
- Chứng minh tenant isolation và API key security.
- Chạy benchmark baseline cho throughput và P95 latency.
- Vận hành local stack ổn định bằng Docker Compose.
- Chạy test/evidence có thể lặp lại.
