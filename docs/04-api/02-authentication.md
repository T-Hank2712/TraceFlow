# Authentication API

Authentication API thuộc ASP.NET Core Control API.

## Endpoint

- `POST /api/v1/auth/register`
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh-token`
- `POST /api/v1/auth/logout`
- `GET /api/v1/auth/me`
- `PUT /api/v1/auth/change-password`
- `PATCH /api/v1/users/me/profile`

## Token

Access token dùng cho request ngắn hạn. Refresh token được rotate khi refresh; nếu một refresh token đã revoke bị tái sử dụng, Control API revoke các refresh token còn hiệu lực của user đó.

## Nguyên tắc

Không để ingestion API dùng user access token. Ingestion API dùng API key riêng của application.
