# Security

Nhóm tài liệu này tập trung vào bảo mật của TraceFlow. Vì TraceFlow lưu log của nhiều project/workspace, lỗi bảo mật quan trọng nhất không chỉ là user đăng nhập sai, mà còn là lộ log giữa tenant, lộ API key, hoặc query OpenSearch thiếu tenant filter.

Security docs phải được đọc song song với architecture, API và data docs. Bảo mật không phải lớp thêm ở cuối; nó ảnh hưởng trực tiếp tới cách thiết kế schema, endpoint, query, ingestion và background job.

## Nội dung cần nắm

- User authentication dùng access token và refresh token.
- Authorization dựa trên workspace membership và role.
- API key là credential cho application gửi log, không phải user token.
- Tenant isolation phải được enforce trong Management API, Search API, Dashboard API, alert evaluation, retention job và usage aggregation.
- Secret management phải tránh commit secret và tránh log credential.
- Security checklist dùng trước khi merge/release các thay đổi nhạy cảm.

## Khi nào cần cập nhật nhóm này

- Khi thêm cơ chế auth hoặc role mới.
- Khi thay đổi API key, token, hashing hoặc secret handling.
- Khi thêm query đọc log hoặc background job quét dữ liệu nhiều project.
- Khi thêm notification channel hoặc external integration.
- Khi phát hiện rủi ro bảo mật mới.

## Thứ tự đọc

1. `01-authentication-and-authorization.md`
2. `02-api-key-security.md`
3. `03-tenant-isolation.md`
4. `04-secret-management.md`
5. `05-security-checklist.md`

## Kết quả mong đợi sau khi đọc

Sau khi đọc xong nhóm này, người đọc phải chỉ ra được nơi nào cần authorization check, nơi nào cần tenant filter và dữ liệu nào tuyệt đối không được log/lưu plaintext.
