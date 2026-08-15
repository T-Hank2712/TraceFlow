# PostgreSQL Schema

PostgreSQL lưu dữ liệu nghiệp vụ và metadata.

## Bảng chính

- `users`
- `refresh_tokens`
- `workspaces`
- `workspace_members`
- `projects`
- `applications`
- `api_keys`
- `alert_rules`
- `notification_channels`
- `retention_policies`
- `usage_counters`
- `audit_logs`

## Nguyên tắc

- Dùng foreign key để bảo vệ quan hệ.
- Dùng unique constraint cho invariant quan trọng.
- Không lưu API key secret plaintext.
- Tất cả bảng chính nên có `created_at` và `updated_at`.
