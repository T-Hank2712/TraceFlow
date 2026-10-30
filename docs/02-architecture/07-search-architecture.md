# Kiến Trúc Search

TraceFlow sử dụng OpenSearch để lưu và tìm kiếm log.

## Truy vấn cần hỗ trợ

- Full-text search theo `message`.
- Filter bắt buộc theo `workspaceId` và `projectId`.
- Filter optional theo `applicationId`, `service`, `environment`, `level`, `traceId` hoặc `correlationId`.
- Filter theo `environment`.
- Filter theo `level`.
- Time range.
- `trace_id`.
- `correlation_id`.
- Sort theo timestamp.
- Pagination.

## Nguyên tắc multi-tenant

Mọi search query phải bắt buộc filter theo project mà user có quyền truy cập. Không được cho phép query toàn index nếu không có ràng buộc tenant.

## Index strategy

Hệ thống nên dùng time-based index kết hợp alias để hỗ trợ retention, rollover và truy vấn theo time range. Chiến lược index phải được benchmark theo log volume thực tế.
