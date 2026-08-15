# OpenAPI Specs

Thư mục này chứa OpenAPI specs cho các nhóm API chính của TraceFlow. Đây là phần contract có thể được dùng bởi tooling để generate client, validate request/response hoặc hỗ trợ contract test.

Các file Markdown trong `04-api/` giải thích ý tưởng và quy tắc. Các file YAML trong thư mục này nên thể hiện contract cụ thể ở dạng máy đọc được: path, method, parameter, request body, response body, status code và schema.

## Nguyên tắc cập nhật

- Khi đổi docs API dạng Markdown, OpenAPI spec tương ứng cũng phải được cập nhật.
- Schema dùng lại nên được đặt vào `components`.
- Error response nên thống nhất với `07-error-response.md`.
- Security scheme phải tách Bearer token và API key.
- Không thêm field vào response nếu chưa xác định rõ ý nghĩa và ownership.

## Khi nào cần cập nhật nhóm này

- Khi thêm endpoint mới.
- Khi đổi path, method, query parameter hoặc header.
- Khi đổi request/response schema.
- Khi đổi authentication scheme.
- Khi thêm error code hoặc status code mới.

## Thứ tự đọc

1. `01-management-api.yaml`
2. `02-ingestion-api.yaml`
3. `03-search-api.yaml`

## Kết quả mong đợi

Khi hoàn thiện, các OpenAPI specs phải đủ rõ để một client có thể tích hợp với TraceFlow mà không cần đọc code backend.
