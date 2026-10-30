# API Key Security

API key là credential cho application gửi log.

## Quy tắc

- Không lưu plaintext secret.
- Không log API key.
- Chỉ hiển thị secret một lần khi tạo.
- Hỗ trợ revoke và rotate.
- Ghi nhận `last_used_at`.
- Scope API key theo application/project.

## Revoke và cache

Control API ghi revoke marker vào Redis khi key bị thu hồi. Ingestion API kiểm tra marker này trước tenant-context cache ở mọi request; vì vậy cache còn TTL không thể cho key đã revoke tiếp tục ingest log.

Nếu Ingestion không thể xác nhận trạng thái revoke qua Redis, nó không tin tenant context cache và chuyển sang validate trực tiếp với Control API. Nếu Control API không xác thực được key, request bị từ chối.

## Proxy và rate limit

Ingestion chỉ tin `X-Forwarded-For` từ reverse-proxy network được cấu hình bằng `TrustedProxies__KnownNetworks__*`. Điều này giữ rate-limit pre-auth theo IP client thật khi đi qua Nginx, đồng thời chặn client gửi trực tiếp header giả để né rate limit.

## Header

```text
Authorization: ApiKey <secret>
```

## Rủi ro

Nếu API key bị lộ, attacker có thể gửi log giả hoặc làm tăng usage. Vì vậy cần rate limit, audit log và khả năng revoke nhanh.
