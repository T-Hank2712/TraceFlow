# Retention Policy

Retention xác định log được lưu bao lâu trước khi xóa.

## Cấp cấu hình

Retention có thể được cấu hình ở cấp project, workspace hoặc plan. Project-level policy là cấu hình cụ thể nhất.

## Giá trị ví dụ

- 7 ngày.
- 30 ngày.
- 90 ngày.

## OpenSearch

Retention có thể được thực hiện bằng index lifecycle hoặc job xóa theo time range. Cần tránh xóa nhầm dữ liệu của project khác.
