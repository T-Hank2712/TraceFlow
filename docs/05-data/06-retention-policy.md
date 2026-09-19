# Retention Policy

Retention xác định log được lưu bao lâu trước khi xóa.

## Cấp cấu hình

Retention trong scope hiện tại nên được giữ đơn giản ở cấp project hoặc application với fixed values như 7/14/30/90 ngày.

## Giá trị ví dụ

- 7 ngày.
- 30 ngày.
- 90 ngày.

## OpenSearch

Retention có thể được thực hiện bằng job xóa theo time range hoặc index lifecycle đơn giản nếu phù hợp. Cần tránh xóa nhầm dữ liệu của project khác. Archive/restore log hết hạn không thuộc scope hiện tại.
