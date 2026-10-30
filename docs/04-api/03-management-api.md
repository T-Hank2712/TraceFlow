# Management API

Management API quản lý workspace, project, application và API key.

## Workspace

- `POST` / `GET /api/v1/workspaces`; `GET`, `PATCH`, `DELETE /api/v1/workspaces/{workspaceId}`.
- Quản lý member, leave, transfer ownership và workspace invitation dưới `/api/v1/workspaces`.

## Project

- `POST` / `GET /api/v1/workspaces/{workspaceId}/projects`.
- `GET`, `PATCH`, `DELETE /api/v1/projects/{projectId}`; member và invitation có route dưới `/api/v1/projects`.

## Application

- `POST` / `GET /api/v1/projects/{projectId}/applications`.
- `GET`, `PATCH`, `DELETE /api/v1/applications/{applicationId}`.

## API Key

- `POST` / `GET /api/v1/applications/{applicationId}/api-keys`.
- `DELETE /api/v1/api-keys/{apiKeyId}` để revoke. Create response là nơi duy nhất trả full secret.
