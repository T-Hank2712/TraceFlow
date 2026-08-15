# Business Flow

Luồng sử dụng cơ bản của TraceFlow:

```text
User
-> Tạo Workspace
-> Tạo Project
-> Đăng ký Application / Service
-> Tạo API Key
-> Application gửi log
-> TraceFlow xử lý và index log
-> User tìm kiếm / lọc / truy vết log
```

## Ví dụ tổ chức dữ liệu

```text
Workspace: KickTicket Team
└── Project: KickTicket Production
    ├── Application: kickticket-api
    ├── Application: payment-worker
    ├── Application: ticket-worker
    └── Application: notification-worker
```

## Nguyên tắc cô lập

Mỗi project phải được cô lập dữ liệu. User thuộc workspace/project này không được đọc log, API key hoặc cấu hình của workspace/project khác.
