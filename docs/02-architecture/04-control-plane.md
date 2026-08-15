# Control Plane

Control plane quản lý metadata và API dành cho người dùng TraceFlow.

## Trách nhiệm

- Đăng ký và đăng nhập user.
- Quản lý workspace và thành viên.
- Quản lý project.
- Quản lý application/service.
- Tạo, revoke và kiểm tra API key.
- Cung cấp Search API và Dashboard API.

## Storage

Control plane sử dụng PostgreSQL cho dữ liệu có tính transaction như user, workspace, project, application, API key, role và cấu hình.

## Nguyên tắc

Mọi truy vấn control plane phải kiểm tra quyền truy cập theo workspace/project. Không được dựa vào client truyền `project_id` mà bỏ qua authorization server-side.
