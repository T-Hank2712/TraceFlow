# Cấu Trúc Tài Liệu TraceFlow

Tài liệu này định nghĩa cấu trúc docs cho toàn bộ project TraceFlow. Mục tiêu là tạo một bản đồ rõ ràng cho sản phẩm hoàn chỉnh, giúp mọi thành phần kỹ thuật đều có lý do, có ranh giới và có contract tài liệu đi kèm.

```text
docs/
├── 00-documentation-structure.md
├── README.md
├── 01-product/
│   ├── README.md
│   ├── 01-overview.md
│   ├── 02-users-and-use-cases.md
│   ├── 03-business-flow.md
│   ├── 04-complete-scope.md
│   └── 05-roadmap.md
├── 02-architecture/
│   ├── README.md
│   ├── 01-system-overview.md
│   ├── 02-service-boundaries.md
│   ├── 03-data-plane.md
│   ├── 04-control-plane.md
│   ├── 05-ingestion-pipeline.md
│   ├── 06-processing-pipeline.md
│   ├── 07-search-architecture.md
│   ├── 08-multi-tenancy.md
│   ├── 09-failure-handling.md
│   ├── 10-backpressure.md
│   ├── 11-observability.md
│   └── 12-architecture-decisions/
│       ├── README.md
│       ├── 01-adr-use-go-for-data-plane.md
│       ├── 02-adr-use-aspnetcore-for-control-plane.md
│       ├── 03-adr-use-kafka-between-ingestion-and-processing.md
│       └── 04-adr-use-opensearch-for-log-search.md
├── 03-domains/
│   ├── README.md
│   ├── 01-identity.md
│   ├── 02-workspace.md
│   ├── 03-project.md
│   ├── 04-application.md
│   ├── 05-api-key.md
│   ├── 06-log.md
│   ├── 07-search.md
│   ├── 08-dashboard.md
│   ├── 09-alerting.md
│   ├── 10-retention.md
│   └── 11-usage-and-quota.md
├── 04-api/
│   ├── README.md
│   ├── 01-conventions.md
│   ├── 02-authentication.md
│   ├── 03-management-api.md
│   ├── 04-ingestion-api.md
│   ├── 05-search-api.md
│   ├── 06-dashboard-api.md
│   ├── 07-error-response.md
│   └── 08-openapi/
│       ├── README.md
│       ├── 01-management-api.yaml
│       ├── 02-ingestion-api.yaml
│       └── 03-search-api.yaml
├── 05-data/
│   ├── README.md
│   ├── 01-postgres-schema.md
│   ├── 02-opensearch-indexes.md
│   ├── 03-kafka-topics.md
│   ├── 04-log-event-schema.md
│   ├── 05-api-key-storage.md
│   ├── 06-retention-policy.md
│   └── 07-sample-data.md
├── 06-services/
│   ├── README.md
│   ├── 01-aspnet-control-api.md
│   ├── 02-go-ingestion-service.md
│   ├── 03-go-log-processor.md
│   └── 04-local-development-stack.md
├── 07-security/
│   ├── README.md
│   ├── 01-authentication-and-authorization.md
│   ├── 02-api-key-security.md
│   ├── 03-tenant-isolation.md
│   ├── 04-secret-management.md
│   └── 05-security-checklist.md
├── 08-operations/
│   ├── README.md
│   ├── 01-docker-compose.md
│   ├── 02-configuration.md
│   ├── 03-migrations.md
│   ├── 04-logging.md
│   ├── 05-metrics.md
│   ├── 06-health-checks.md
│   ├── 07-backup-and-restore.md
│   └── 08-troubleshooting.md
├── 09-testing/
│   ├── README.md
│   ├── 01-strategy.md
│   ├── 02-unit-tests.md
│   ├── 03-integration-tests.md
│   ├── 04-end-to-end-tests.md
│   ├── 05-contract-tests.md
│   ├── 06-performance-tests.md
│   └── 07-test-data.md
├── 10-development/
│   ├── README.md
│   ├── 01-setup.md
│   ├── 02-coding-standards.md
│   ├── 03-git-workflow.md
│   ├── 04-branching-strategy.md
│   ├── 05-commit-conventions.md
│   └── 06-definition-of-done.md
└── 11-references/
    ├── README.md
    ├── 01-glossary.md
    ├── 02-system-design-notes.md
    ├── 03-kafka-notes.md
    ├── 04-opensearch-notes.md
    └── 05-performance-benchmark-notes.md
```

## Mô Tả Từng Nhóm Tài Liệu

### `docs/README.md`

Trang mở đầu của bộ tài liệu. Giải thích TraceFlow là gì, nên đọc tài liệu theo thứ tự nào và liên kết đến các nhóm tài liệu quan trọng.

### `01-product/`

Mô tả TraceFlow dưới góc nhìn sản phẩm: người dùng, use case, business flow, phạm vi sản phẩm hoàn chỉnh và roadmap triển khai.

### `02-architecture/`

Mô tả kiến trúc tổng thể và các quyết định hệ thống: service boundary, data plane, control plane, ingestion, processing, search, multi-tenancy, failure handling, backpressure và observability.

### `02-architecture/12-architecture-decisions/`

Lưu các ADR. Mỗi file ghi một quyết định kỹ thuật quan trọng, lý do chọn, phương án thay thế và hệ quả.

### `03-domains/`

Mô tả từng domain trong hệ thống: Identity, Workspace, Project, Application, API Key, Log, Search, Dashboard, Alerting, Retention và Usage/Quota.

### `04-api/`

Mô tả contract API của TraceFlow, bao gồm convention chung, authentication, management API, ingestion API, search API, dashboard API, error response và OpenAPI specs.

### `05-data/`

Mô tả thiết kế dữ liệu: PostgreSQL schema, OpenSearch index mapping, Kafka topic, log event schema, API key storage, retention policy và sample data.

### `06-services/`

Mô tả từng service chính: ASP.NET Core Control API, Go Ingestion Service, Go Log Processor và local development stack.

### `07-security/`

Tập trung vào authentication, authorization, API key security, tenant isolation, secret management và checklist bảo mật.

### `08-operations/`

Mô tả cách vận hành hệ thống: Docker Compose, configuration, migration, logging, metrics, health checks, backup/restore và troubleshooting.

### `09-testing/`

Mô tả chiến lược test: unit, integration, end-to-end, contract, performance và test data.

### `10-development/`

Mô tả quy tắc làm việc trong repo: setup môi trường, coding standards, git workflow, branching, commit conventions và definition of done.

### `11-references/`

Lưu ghi chú học tập và tham khảo về system design, Kafka, OpenSearch, benchmark và glossary.

## Nguyên Tắc Viết Docs

1. Mỗi tài liệu phải có mục đích rõ ràng.
2. Tài liệu mô tả trạng thái hoàn chỉnh của project.
3. Roadmap chỉ mô tả thứ tự triển khai, không làm giảm phạm vi thiết kế.
4. Kiến trúc và implementation phải đồng bộ với nhau.
5. Mỗi quyết định lớn phải có ADR.
6. Mọi công nghệ được thêm vào phải giải thích nó giải quyết vấn đề nào.
7. Tài liệu phải luôn bảo vệ các yêu cầu cốt lõi: correctness, tenant isolation, security, failure handling và observability.
