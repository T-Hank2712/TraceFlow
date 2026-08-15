# Backup Và Restore

TraceFlow cần chiến lược backup cho metadata và cấu hình.

## PostgreSQL

Backup user, workspace, project, application, API key metadata, alert rule, retention policy và usage/quota.

## OpenSearch

Log có thể rất lớn. Chính sách backup OpenSearch phụ thuộc retention, cost và yêu cầu khôi phục.

## Nguyên tắc

Restore phải được kiểm thử định kỳ. Backup không có giá trị nếu chưa từng restore thử.
