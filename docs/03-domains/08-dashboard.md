# Domain: Dashboard / Operational Insights

Dashboard/Operational Insights là nice-to-have. Capability này cung cấp số liệu tổng quan từ log, nhưng không phải điều kiện để TraceFlow Core hoàn thiện.

## Metrics

- Total logs.
- Logs per second.
- Errors.
- Warnings.
- Error rate.
- Logs by service.
- Logs by level.
- Recent errors.
- Top error messages.

## Filter

- Workspace.
- Project.
- Environment.
- Time range.

## Nguyên tắc

Dashboard không thay thế search. Trong scope hiện tại, ưu tiên backend summary API nhỏ như log volume, error count, error rate và top services. Advanced dashboard là optional.
