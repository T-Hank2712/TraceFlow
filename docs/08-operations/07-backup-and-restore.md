# Backup Và Restore

Backup/Restore là optional operations topic. Nó hữu ích khi muốn bổ sung khả năng vận hành, nhưng không phải điều kiện hoàn thiện TraceFlow Core.

## PostgreSQL

Nếu triển khai, ưu tiên backup user, workspace, project, application và API key metadata. Retention policy, usage/quota hoặc alert rule chỉ cần backup nếu các capability đó được bật.

## OpenSearch

Log có thể rất lớn. Trong scope hiện tại không làm archive/restore log hết hạn. Chính sách backup OpenSearch nếu có phụ thuộc retention, cost và yêu cầu demo/vận hành.

## Nguyên tắc

Restore phải được kiểm thử định kỳ. Backup không có giá trị nếu chưa từng restore thử.
