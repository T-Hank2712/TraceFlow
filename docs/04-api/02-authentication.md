# Authentication API

Authentication API thuộc ASP.NET Core Control API.

## Endpoint

- `POST /auth/register`
- `POST /auth/login`
- `POST /auth/refresh`
- `POST /auth/logout`
- `GET /me`

## Token

Access token dùng cho request ngắn hạn. Refresh token dùng để lấy access token mới và có thể revoke.

## Nguyên tắc

Không để ingestion API dùng user access token. Ingestion API dùng API key riêng của application.
