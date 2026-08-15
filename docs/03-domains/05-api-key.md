# Domain: API Key

API Key cho phép application gửi log vào TraceFlow.

## Yêu cầu bảo mật

- Không lưu plaintext secret trong database.
- Chỉ hiển thị secret một lần khi tạo.
- Có thể revoke.
- Có trạng thái `active` hoặc `revoked`.
- Có `created_at` và `revoked_at` nếu bị thu hồi.

## Quy tắc

API key thuộc một application và gián tiếp thuộc một project. Ingestion service dùng API key để xác định `project_id` và `application_id`, không tin dữ liệu tenant do client gửi lên.

## Năng lực cần có

API key cần hỗ trợ tạo, hash secret, lưu prefix để nhận diện, validate key, revoke key, rotate key và theo dõi `last_used_at`.
