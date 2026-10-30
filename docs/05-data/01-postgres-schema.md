# PostgreSQL Schema

PostgreSQL lưu dữ liệu nghiệp vụ và metadata.

## Bảng chính

- `Users`
- `RefreshTokens`
- `Workspaces`
- `WorkspaceMembers`
- `WorkspaceInvitations`
- `Projects`
- `ProjectMembers`
- `ProjectInvitations`
- `TraceApplications`
- `ApiKeys`

## Bảng nice-to-have / optional

- `retention_policies` nếu triển khai retention.
- `usage_counters` nếu triển khai quota/usage tracking dài hạn.
- `alert_rules` và `notification_channels` chỉ thuộc optional alerting.

## Nguyên tắc

- Dùng foreign key để bảo vệ quan hệ.
- Dùng unique constraint cho invariant quan trọng.
- Không lưu API key secret plaintext.
- Tất cả bảng chính nên có `created_at` và `updated_at`.
