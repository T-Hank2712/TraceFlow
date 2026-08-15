# API Key Security

API key là credential cho application gửi log.

## Quy tắc

- Không lưu plaintext secret.
- Không log API key.
- Chỉ hiển thị secret một lần khi tạo.
- Hỗ trợ revoke và rotate.
- Ghi nhận `last_used_at`.
- Scope API key theo application/project.

## Header

```text
Authorization: ApiKey <secret>
```

## Rủi ro

Nếu API key bị lộ, attacker có thể gửi log giả hoặc làm tăng usage. Vì vậy cần rate limit, audit log và khả năng revoke nhanh.
