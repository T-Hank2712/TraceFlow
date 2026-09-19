# Roadmap

## Milestone 1 - Core Foundation

Tập trung xây nền tảng tenant và credential:

- Auth/RBAC.
- Workspace/Project/Application.
- API Key lifecycle.
- Internal API key validation.

## Milestone 2 - Core Log Pipeline

Tập trung hoàn thành luồng log cốt lõi:

```text
Project -> Application -> API Key -> Log Ingestion -> Kafka -> Processor -> OpenSearch -> Search
```

Bao gồm Ingestion API, Kafka publish, Log Processor, batch buffer, OpenSearch Bulk API và Search API.

## Milestone 3 - Reliability Và Redis Protection

- Retry policy.
- Dead Letter Queue.
- Full bulk failure handling.
- Partial bulk failure handling.
- Redis validation cache.
- Redis rate limit/counter ngắn hạn.
- Security/tenancy verification.

## Milestone 4 - Verification Và Nice-To-Have

- End-to-end tests.
- Reliability tests.
- Performance benchmark có báo cáo.
- Retention policy đơn giản nếu cần.
- Quota bảo vệ ingestion nếu cần.
- Operational Insights API nếu cần.
- Minimal Web UI nếu cần demo.

## Optional - Polish Sau Core

- Alerting.
- Backup/restore.
- Advanced dashboard.

Các phần optional chỉ làm khi core pipeline đã ổn định và có evidence rõ ràng.
