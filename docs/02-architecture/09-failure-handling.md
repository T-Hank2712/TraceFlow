# Failure Handling

TraceFlow phải định nghĩa rõ hành vi khi từng thành phần lỗi.

## Kafka unavailable

Ingestion service không thể đảm bảo log đã được nhận. API nên trả lỗi `503 Service Unavailable` hoặc lỗi tương đương, kèm message rõ ràng.

## OpenSearch unavailable

Processor retry với backoff. Nếu vượt quá giới hạn, batch cần được đưa vào DLQ hoặc giữ offset chưa commit tùy policy.

## Invalid log

Log invalid ở public API bị reject bằng `400 Bad Request`. Log invalid trong Kafka cần được ghi nhận, đo đếm và có thể đưa vào DLQ.

## Consumer crash

Consumer phải hỗ trợ graceful shutdown. Offset management phải tránh mất log khi crash giữa batch.

## Duplicate log

Hệ thống sử dụng at-least-once delivery. Client gửi `Idempotency-Key` ổn định cho mỗi logical request; Ingestion lưu ánh xạ key và payload trong Redis rồi dùng lại cùng ULID `eventId` cho mọi retry trong 24 giờ. Processor dùng `eventId` làm OpenSearch document ID, nên bản ghi Kafka trùng do retry ghi đè cùng document thay vì tạo document mới. Dùng lại idempotency key với payload khác bị từ chối bằng `409 Conflict`.
