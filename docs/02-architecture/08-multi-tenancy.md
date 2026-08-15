# Multi-Tenancy

TraceFlow là hệ thống nhiều workspace và nhiều project. Cô lập dữ liệu là yêu cầu bắt buộc.

## Tenant boundary

Boundary chính là `workspace_id` và `project_id`. Mỗi log phải thuộc đúng một project và một application.

## Quy tắc

- User chỉ được truy cập workspace mà họ là member.
- User chỉ được đọc project thuộc workspace của họ.
- Search API phải filter theo project được authorize.
- API key chỉ được dùng để gửi log cho project/application tương ứng.
- Không tin tưởng `project_id` do client ingestion gửi lên.

## Rủi ro

Lỗi multi-tenancy thường không gây crash, nhưng gây lộ dữ liệu. Vì vậy cần test authorization và tenant isolation trong toàn bộ API, search query, dashboard query và background job.
