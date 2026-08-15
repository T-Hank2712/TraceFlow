# ASP.NET Core Control API

ASP.NET Core Control API là service quản lý control plane của TraceFlow.

## Trách nhiệm

- Authentication và refresh token.
- Workspace, member và role.
- Project, application và API key.
- Search API và Dashboard API.
- Alert rule, notification channel, retention policy và usage/quota.

## Dependency

- PostgreSQL cho metadata nghiệp vụ.
- OpenSearch cho search/dashboard query.
- Redis nếu cần caching, rate limiting hoặc counter ngắn hạn.

## Nguyên tắc

Control API phải enforce authorization ở server-side cho mọi workspace/project. Không query OpenSearch nếu chưa xác thực quyền truy cập project.
