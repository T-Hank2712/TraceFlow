# OpenSearch Indexes

OpenSearch lưu log phục vụ search. Operational insights có thể đọc aggregation từ OpenSearch nếu nice-to-have này được bật.

## Trường index chính

- `timestamp`
- `workspaceId`
- `projectId`
- `applicationId`
- `service`
- `environment`
- `level`
- `message`
- `traceId`
- `correlationId`
- `metadata`

## Mapping nguyên tắc

- Control API hiện chỉ filter/sort; chưa gửi full-text query cho `message`.
- `workspaceId`, `projectId`, `applicationId`, `environment`, `service`, `traceId` và `correlationId` dùng `keyword`; `level` dùng `integer`.
- `timestamp` dùng date cho range query và sort.
- Processor hiện index `LogEvent`, không giữ `receivedAt` hoặc `processedAt` trong document.
- Mapping index được processor khởi tạo với `dynamic: false`.
- `metadata` dùng `flat_object` để metadata key tùy ý không tạo field mapping mới. Metadata vẫn phù hợp cho lookup đơn giản, nhưng không phải nơi khai báo field có mapping chuyên biệt.
- Processor fail fast nếu index hiện có không dùng mapping an toàn. Không thể đổi loại field của index đang tồn tại tại chỗ; dùng index mới/reindex rồi cập nhật `OpenSearch__Index` khi cần migration.
