# Domain: Project

Project là boundary chính để cô lập log và search.

## Entity chính

- Project.
- Workspace.
- Retention Configuration.

## Quy tắc

- Project luôn thuộc một workspace.
- Log luôn thuộc một project.
- Search API phải kiểm tra quyền trên project.
- Tên project nên unique trong cùng workspace.

## Năng lực cần có

Project cần hỗ trợ tạo, cập nhật, lưu cấu hình retention, quản lý application và dùng `project_id` làm tenant boundary trong search.
