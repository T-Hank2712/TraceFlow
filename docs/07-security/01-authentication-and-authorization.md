# Authentication Và Authorization

TraceFlow có hai cơ chế xác thực chính: user token cho control/search API và API key cho ingestion API.

## User authentication

User đăng nhập để nhận access token và refresh token. Access token dùng để gọi API quản lý workspace, project, search và dashboard.

## Authorization

Quyền truy cập dựa trên workspace membership và role. Mọi thao tác trên project phải kiểm tra project thuộc workspace mà user có quyền.

## Nguyên tắc

Không tin `workspace_id` hoặc `project_id` từ client nếu chưa kiểm tra quan hệ trong database.
