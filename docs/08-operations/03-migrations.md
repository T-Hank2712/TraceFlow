# Migrations

Migration quản lý thay đổi schema PostgreSQL.

## Nguyên tắc

- Migration phải deterministic.
- Không sửa migration đã chạy ở môi trường chia sẻ.
- Constraint quan trọng phải nằm trong database, không chỉ trong code.
- Migration phá vỡ backward compatibility cần kế hoạch triển khai.

## Kiểm tra

Mọi migration cần được chạy trong integration test hoặc local stack trước khi merge.
