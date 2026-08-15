# Tenant Isolation

Tenant isolation đảm bảo dữ liệu workspace/project này không bị truy cập bởi workspace/project khác.

## Điểm kiểm soát

- Management API.
- Search API.
- Dashboard API.
- Alert evaluation.
- Retention job.
- Usage aggregation.
- OpenSearch query filter.

## Nguyên tắc

Mọi log document trong OpenSearch phải có `workspace_id` hoặc `project_id`. Mọi query phải thêm filter tenant từ server-side authorization context.
