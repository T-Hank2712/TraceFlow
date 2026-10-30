# API Conventions

## Format

- Request/response dùng JSON.
- Timestamp dùng ISO 8601 UTC.
- ID dùng ULID, được biểu diễn thành string trong JSON.
- Header auth cho user dùng Bearer token.
- Header auth cho ingestion dùng API key.

## Pagination

Search API hiện dùng offset pagination với `page` và `pageSize`; `pageSize` hợp lệ từ `1` đến `100`.

## Error

Control API trả `ProblemDetails` hoặc `ValidationProblemDetails`. Ingestion API trả JSON gồm `code`, `message` và `details` khi có.
