# API Conventions

## Format

- Request/response dùng JSON.
- Timestamp dùng ISO 8601 UTC.
- ID dùng UUID.
- Header auth cho user dùng Bearer token.
- Header auth cho ingestion dùng API key.

## Pagination

Search API nên dùng cursor-based pagination với `limit` và `cursor` để tránh phân trang sâu gây chậm trên OpenSearch.

## Error

Mọi lỗi trả về format thống nhất gồm `code`, `message` và `details` nếu cần.
