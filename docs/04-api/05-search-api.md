# Search API

Search API do Control API cung cấp và truy vấn OpenSearch.

## Endpoint

- `GET /api/v1/workspaces/{workspaceId}/projects/{projectId}/logs`

## Filter

- `service`
- `applicationId`
- `environment`
- `level`
- `from`
- `to`
- `correlationId`
- `page`
- `pageSize`

## Nguyên tắc

Trước khi query OpenSearch, API kiểm tra user là project member và luôn thêm `workspaceId` cùng `projectId` vào filter. API hiện không có full-text `q`, trace lookup endpoint riêng hoặc cursor pagination.
