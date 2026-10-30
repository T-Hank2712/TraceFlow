# Domain: Retention

Retention kiểm soát thời gian lưu log. Đây là nice-to-have, không phải điều kiện để core ingestion/search pipeline hoạt động.

## Chính sách ví dụ

- Free: 7 ngày.
- Pro: 30 ngày.
- Enterprise: 90 ngày hoặc tùy chỉnh.

## Quy tắc

- Retention ưu tiên thuộc project hoặc application với fixed values đơn giản.
- Không lưu log vô hạn.
- Xóa dữ liệu cũ phải có policy rõ, có thể kiểm tra và quan sát được.

## Năng lực cần có

Retention cần có policy rõ ràng và job thực thi an toàn. Archive/restore log hết hạn không nằm trong scope hiện tại.
