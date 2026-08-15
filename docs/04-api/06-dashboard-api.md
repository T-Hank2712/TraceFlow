# Dashboard API

Dashboard API cung cấp số liệu tổng quan cho project.

## Endpoint

- `GET /projects/{projectId}/dashboard/summary`
- `GET /projects/{projectId}/dashboard/logs-by-level`
- `GET /projects/{projectId}/dashboard/logs-by-service`
- `GET /projects/{projectId}/dashboard/recent-errors`

## Filter

- `environment`
- `from`
- `to`

## Nguyên tắc

Dashboard API lấy dữ liệu từ OpenSearch aggregation hoặc query tối ưu tương đương.
