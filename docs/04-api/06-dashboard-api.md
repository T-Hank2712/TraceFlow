# Dashboard / Operational Insights API

Dashboard/Operational Insights API là nice-to-have. API này cung cấp số liệu tổng quan cho project, nhưng không phải điều kiện để core ingestion/search pipeline hoàn thiện.

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

API này lấy dữ liệu từ OpenSearch aggregation hoặc query tối ưu tương đương. Scope ban đầu nên giữ nhỏ: volume, error count, error rate, top services và recent errors. Advanced dashboard không thuộc core.
