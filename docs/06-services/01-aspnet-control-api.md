# ASP.NET Core Control API

ASP.NET Core Control API là service quản lý control plane của TraceFlow.

## Trách nhiệm

- Authentication và refresh token.
- Workspace, member và role.
- Project, application và API key.
- Search API.
- Internal API key validation cho Ingestion API.

## Dependency

- PostgreSQL cho metadata nghiệp vụ.
- OpenSearch cho search query.
- Control API hiện không dùng Redis; cache validation và ingestion rate limit thuộc Ingestion API.

## Nguyên tắc

Control API phải enforce authorization ở server-side cho mọi workspace/project. Không query OpenSearch nếu chưa xác thực quyền truy cập project.
