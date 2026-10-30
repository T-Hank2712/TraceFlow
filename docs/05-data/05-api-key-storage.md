# API Key Storage

API key không được lưu plaintext.

## Thiết kế đề xuất

- Secret đầy đủ chỉ hiển thị một lần khi tạo.
- Database lưu hash của secret.
- Database lưu prefix để hiển thị và lookup thuận tiện.
- Key có trạng thái `active` hoặc `revoked`.

## Trường dữ liệu

- `id`
- `TraceApplicationId`
- `key_prefix`
- `secret_hash`
- `environment`
- `status`
- `created_at`
- `revoked_at`
- `last_used_at`

## Nguyên tắc

Hash dùng SHA-256 với pepper cấu hình ở server. Không log API key trong request log.
