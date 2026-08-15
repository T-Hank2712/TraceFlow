# TraceFlow

TraceFlow là nền tảng centralized logging và observability backend cho các hệ thống nhiều service.

Nền tảng này thu thập structured logs từ nhiều ứng dụng, xử lý log qua pipeline bất đồng bộ, index dữ liệu vào search engine và cung cấp API để tìm kiếm, lọc, truy vết và phân tích log theo ngữ cảnh vận hành.

```text
Applications -> Ingestion API -> Kafka -> Log Processor -> OpenSearch -> TraceFlow API
```

## Tổng Quan

TraceFlow được thiết kế để hỗ trợ đội kỹ thuật theo dõi và điều tra sự cố trong môi trường distributed services mà không cần truy cập từng server, container hoặc service riêng lẻ.

Hệ thống tập trung vào ba năng lực cốt lõi:

- **Log ingestion**: tiếp nhận single log và batch logs từ nhiều application thông qua API key.
- **Log processing**: xử lý bất đồng bộ qua Kafka, chuẩn hóa dữ liệu, batching, retry và DLQ.
- **Log search & observability**: tìm kiếm full-text, filter theo ngữ cảnh, trace lookup, dashboard metrics và alerting.

## Kiến Trúc

TraceFlow tách hệ thống thành hai mặt phẳng trách nhiệm:

```text
Control Plane  -> ASP.NET Core
Data Plane     -> Go
```

**Control Plane** quản lý authentication, workspace, project, application, API key, search API, dashboard API, alert rules, retention policy và usage/quota.

**Data Plane** xử lý log ingestion, Kafka producer/consumer, batch processing, failure handling và OpenSearch indexing.

## Thành Phần Chính

- **ASP.NET Core API**: control plane, search API và dashboard API.
- **Go Ingestion Service**: nhận log, xác thực API key, validate payload và publish vào Kafka.
- **Go Log Processor**: consume Kafka, xử lý batch, retry/DLQ và index vào OpenSearch.
- **PostgreSQL**: lưu dữ liệu nghiệp vụ và metadata.
- **Kafka**: buffer và stream trung gian giữa ingestion và processing.
- **OpenSearch**: lưu trữ log phục vụ search, trace lookup và analytics.
- **Redis**: cache, rate limiting và counter ngắn hạn khi cần.

## Khả Năng Hệ Thống

- Multi-tenant workspace/project isolation.
- API key based log submission.
- Structured logging với metadata linh hoạt.
- Full-text search và filter theo `service`, `environment`, `level`, `timestamp`, `trace_id`, `correlation_id`.
- Trace search để theo dõi request xuyên nhiều service.
- Dashboard metrics cho log volume, error rate, service errors và top error messages.
- Alerting theo rule, threshold và time window.
- Retention policy, usage tracking và quota.
- Failure handling với retry, DLQ, backpressure và graceful shutdown.
- Observability nội bộ thông qua logs, metrics và health checks.

## Công Nghệ

```text
C# / ASP.NET Core   Control plane APIs
Go                  Ingestion và processing services
PostgreSQL          Transactional metadata
Kafka               Log stream và buffering
OpenSearch          Log search và analytics
Redis               Cache, rate limiting, counters
Docker Compose      Local development stack
```

## Cấu Trúc Repo

```text
docs/          Tài liệu thiết kế, kiến trúc và vận hành
services/      ASP.NET Core và Go services
contracts/     OpenAPI specs, Kafka schemas, OpenSearch mappings
deployments/   Docker Compose và cấu hình triển khai
scripts/       Script hỗ trợ phát triển
tests/         Integration, end-to-end và performance tests
tools/         Công cụ hỗ trợ phát triển
```

## Tài Liệu

- [Cấu trúc tài liệu](docs/00-documentation-structure.md)
- [Tổng quan tài liệu](docs/README.md)
