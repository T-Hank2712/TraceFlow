# Domain: Identity

Identity quản lý user, đăng nhập, refresh token và quyền truy cập cơ bản.

## Entity chính

- User.
- Credential.
- Refresh Token.
- Workspace Member.

## Quy tắc

- Email user phải unique.
- Password không được lưu plaintext.
- Refresh token phải có thời hạn và có thể revoke.
- Authorization phải dựa trên membership trong workspace.

## Năng lực cần có

Identity cần hỗ trợ đăng ký, đăng nhập, refresh token, logout, quản lý phiên, đổi mật khẩu và lấy thông tin user hiện tại.
