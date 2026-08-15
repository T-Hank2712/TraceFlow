# Search API

Search API do ASP.NET Core API cung cấp và truy vấn OpenSearch.

## Endpoint

- `GET /projects/{projectId}/logs`
- `GET /projects/{projectId}/traces/{traceId}`

## Filter

- `q`
- `service`
- `application_id`
- `environment`
- `level`
- `from`
- `to`
- `correlation_id`
- `limit`
- `cursor`

## Nguyên tắc

Trước khi query OpenSearch, API phải kiểm tra user có quyền truy cập project.
