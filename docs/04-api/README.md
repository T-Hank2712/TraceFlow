# API Documentation

Nhóm tài liệu này mô tả contract API của TraceFlow. API docs là ranh giới giữa client, dashboard, Go data plane, ASP.NET Core control plane và các công cụ test/automation.

Vì TraceFlow có cả user-facing API và ingestion API, tài liệu API phải tách rõ hai kiểu xác thực: user access token cho management/search/dashboard và API key cho application gửi log. Không được trộn hai cơ chế này.

## Nhóm API

- Management API: user, workspace, project, application, API key.
- Ingestion API: endpoint nhận log từ application.
- Search API: tìm kiếm và truy vết log.
- Dashboard API: số liệu tổng quan.

## Nội dung cần nắm

- Convention chung về JSON, timestamp, ID, pagination và error format.
- Authentication API quản lý đăng nhập, refresh token và user context.
- Management API quản lý workspace/project/application/API key.
- Ingestion API ưu tiên response nhanh, validate payload và publish Kafka.
- Search API bắt buộc enforce tenant filter trước khi query OpenSearch.
- Dashboard API dùng query/aggregation phục vụ số liệu tổng quan.
- OpenAPI specs là contract máy đọc được để hỗ trợ client/test.

## Khi nào cần cập nhật nhóm này

- Khi thêm, đổi hoặc xóa endpoint.
- Khi đổi request/response schema.
- Khi đổi error code hoặc pagination.
- Khi đổi authentication/authorization behavior.
- Khi Kafka event schema hoặc OpenSearch document ảnh hưởng tới response API.

## Nguyên tắc

API contract phải ổn định trước khi các service phụ thuộc vào nhau. Mọi thay đổi request/response quan trọng cần cập nhật docs và OpenAPI spec.

## Thứ tự đọc

1. `01-conventions.md`
2. `02-authentication.md`
3. `03-management-api.md`
4. `04-ingestion-api.md`
5. `05-search-api.md`
6. `06-dashboard-api.md`
7. `07-error-response.md`
8. `08-openapi/README.md`

## Kết quả mong đợi sau khi đọc

Sau khi đọc xong nhóm này, người đọc phải biết endpoint nào thuộc service nào, cơ chế auth nào được dùng, request/response cần có field gì và thay đổi API nào sẽ kéo theo cập nhật OpenAPI, test contract hoặc schema dữ liệu.
