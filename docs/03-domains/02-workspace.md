# Domain: Workspace

Workspace đại diện cho một tổ chức, đội nhóm hoặc không gian làm việc.

## Entity chính

- Workspace.
- Workspace Member.
- Workspace Role.

## Quy tắc

- Một user có thể thuộc nhiều workspace.
- Một workspace có thể có nhiều project.
- Workspace owner có quyền quản lý member.
- Không được truy cập project nếu không thuộc workspace tương ứng.

## Năng lực cần có

Workspace cần hỗ trợ tạo workspace, xem danh sách workspace, quản lý member, phân vai trò và chuyển quyền ownership khi cần.
