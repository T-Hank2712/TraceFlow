# OpenAPI Contracts

Thư mục này mô tả HTTP API contract của TraceFlow. Đây là contract giữa client, Control API, Ingestion API và các internal service.

## Control API

Control API dùng versioned route dạng `api/v{version}` và mặc định yêu cầu JWT Bearer token, trừ các endpoint được đánh dấu public rõ ràng.

Nhóm endpoint chính:

- `POST /api/v{version}/auth/register`: đăng ký tài khoản.
- `POST /api/v{version}/auth/login`: đăng nhập và nhận access token, refresh token.
- `POST /api/v{version}/auth/refresh-token`: rotate refresh token và cấp access token mới.
- `POST /api/v{version}/auth/logout`: revoke refresh token hiện tại.
- `PUT /api/v{version}/auth/change-password`: đổi mật khẩu và revoke refresh token còn hiệu lực.
- `GET /api/v{version}/auth/me`: lấy thông tin người dùng hiện tại.
- `PATCH /api/v{version}/users/me/profile`: cập nhật hồ sơ người dùng.
- `api/v{version}/workspaces`: quản lý workspace, member, role, leave và transfer ownership.
- `api/v{version}/workspace-invitations`: gửi, xem, accept, decline và hủy workspace invitation.
- `api/v{version}/workspaces/{workspaceId}/projects`: quản lý project, member, role và leave project.
- `api/v{version}/project-invitations`: gửi, xem, accept, decline và hủy project invitation.
- `api/v{version}/workspaces/{workspaceId}/projects/{projectId}/applications`: quản lý trace application.
- `api/v{version}/workspaces/{workspaceId}/projects/{projectId}/applications/{applicationId}/api-keys`: tạo, liệt kê và revoke API key.
- `GET /api/v{version}/workspaces/{workspaceId}/projects/{projectId}/logs`: tìm kiếm log trong OpenSearch.
- `GET /api/system/status`: endpoint public để kiểm tra trạng thái service.

## Internal Control API

Internal API chỉ dành cho service-to-service communication.

- `POST /internal/v{version}/api-keys/validate`
- Header bắt buộc: `X-Internal-Secret`
- Request body:

```json
{
  "apiKey": "tfk_..."
}
```

- Response body:

```json
{
  "valid": true,
  "workspaceId": "01J...",
  "projectId": "01J...",
  "applicationId": "01J...",
  "environment": "production"
}
```

## Ingestion API

Ingestion API nhận log từ application bên ngoài. API này xác thực bằng API key, không dùng JWT.

Header bắt buộc:

```http
Authorization: ApiKey <api-key>
```

### Gửi một log

`POST /v1/logs`

Request body:

```json
{
  "timestamp": "2026-09-19T10:15:30Z",
  "level": 4,
  "service": "checkout-api",
  "message": "Payment provider timeout",
  "traceId": "trace-7f9c2a",
  "correlationId": "order-10001",
  "metadata": {
    "orderId": "10001",
    "provider": "stripe"
  }
}
```

Response body:

```json
{
  "eventId": "01J...",
  "status": true,
  "acceptedAt": "2026-09-19T10:15:31Z"
}
```

### Gửi batch log

`POST /v1/batch-logs`

Request body:

```json
{
  "logs": [
    {
      "timestamp": "2026-09-19T10:15:30Z",
      "level": 2,
      "service": "checkout-api",
      "message": "Order created",
      "traceId": "trace-7f9c2a",
      "correlationId": "order-10001",
      "metadata": {
        "orderId": "10001"
      }
    }
  ]
}
```

Response body:

```json
{
  "batchId": "01J...",
  "total": 1,
  "accepted": 1,
  "rejected": 0,
  "results": [
    {
      "index": 0,
      "accepted": true,
      "eventId": "01J...",
      "error": null
    }
  ]
}
```

## Error response

Control API trả lỗi theo `ProblemDetails` hoặc `ValidationProblemDetails`.

Ingestion API trả lỗi dạng:

```json
{
  "code": "invalid_api_key",
  "message": "Invalid API key.",
  "details": null
}
```

## Quy ước bảo mật

- Public endpoint phải được khai báo rõ bằng `AllowAnonymous`.
- Protected endpoint phải đi qua fallback authorization policy.
- Endpoint nhạy cảm có rate limit riêng; các endpoint còn lại dùng global rate limit.
- Request body limit được cấu hình bằng môi trường, không hard-code trong contract.
- Internal endpoint không được public qua reverse proxy.
