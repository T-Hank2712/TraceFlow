# ADR-0004: Dùng OpenSearch Cho Log Search

## Trạng thái

Chấp nhận.

## Bối cảnh

TraceFlow cần full-text search theo message, filter theo nhiều trường, time range query, trace lookup và pagination.

## Quyết định

Sử dụng OpenSearch làm search backend cho log.

## Lý do

- Hỗ trợ full-text search.
- Hỗ trợ filter và sort theo timestamp.
- Có bulk indexing.
- Phù hợp với log analytics use case.

## Hệ quả

PostgreSQL không lưu log chính. Cần thiết kế index mapping, retention và query pattern cẩn thận để tránh lộ dữ liệu giữa project.
