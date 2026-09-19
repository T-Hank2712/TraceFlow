# Domain: Usage Và Quota

Usage/Quota là nice-to-have để bảo vệ ingestion path, không phải billing system.

Usage dùng để theo dõi mức sử dụng của workspace/project.

## Chỉ số dự kiến

- Số log đã ingest.
- Bytes ingested.
- Storage usage.
- Ingestion requests.
- Search requests.

## Quota ví dụ

- Số GB mỗi tháng.
- Số log mỗi ngày.
- Retention tối đa.
- Số application trong project.

## Năng lực cần có

Usage cần ghi nhận counter theo workspace/project/application/API key khi cần. Quota trong scope hiện tại chỉ nhằm bảo vệ ingestion path, không làm pricing plan, invoice hoặc billing.
