# Contracts

## 1. Document Purpose

Tài liệu này định nghĩa contract chính giữa các thành phần của TraceFlow Core:

```text
Multi-tenant log ingestion and search pipeline
```

Contracts không mô tả toàn bộ endpoint CRUD chi tiết. Tài liệu này chỉ chốt những contract ảnh hưởng trực tiếp tới boundary giữa Control API, Ingestion API, Kafka, Log Processor, OpenSearch, Redis và client/user.

## 2. Contract Boundaries

| Boundary | Producer | Consumer | Contract |
| :--- | :--- | :--- | :--- |
| Public Control API | Control API | Web UI, API client, user | JSON HTTP API, JWT auth, standard error. |
| Public Ingestion API | Ingestion API | Client Application | JSON HTTP API, API key auth. |
| Internal Validation API | Control API | Ingestion API | Internal JSON HTTP API, tenant context response. |
| Redis Cache/Rate Limit | Control API/Ingestion API | Control API/Ingestion API | Key naming, TTL, invalidation expectations. |
| Kafka Log Event | Ingestion API | Log Processor | Enriched log event schema. |
| OpenSearch Document | Log Processor | Control API Search | Indexed log document schema. |
| DLQ Event | Log Processor | Debug/replay tooling | Dead letter event schema. |

Optional features như alerting, backup/restore và advanced dashboard không có contract bắt buộc trong core design.

## 3. Common Conventions

TraceFlow dùng JSON cho HTTP request/response và Kafka event. JSON field dùng `camelCase`. Timestamp dùng RFC3339 UTC. Resource ID dùng string dạng ULID.

### 3.1. Authentication

| Boundary | Header | Rule |
| :--- | :--- | :--- |
| User-facing Control API | `Authorization: Bearer {accessToken}` | JWT đại diện user session. |
| Public Ingestion API | `Authorization: ApiKey {secret}` | API key đại diện client application. |
| Internal service-to-service API | `X-Internal-Secret: {secret}` | Chỉ service nội bộ được gọi. |

JWT không được dùng để gửi log. API key không được dùng để gọi management hoặc search API.

### 3.2. Standard Error

```json
{
  "code": "validation_error",
  "message": "Request payload is invalid.",
  "details": [
    {
      "field": "logs[0].level",
      "message": "Level must be one of DEBUG, INFO, WARN, ERROR, FATAL."
    }
  ],
  "requestId": "01J8Z7Y3YF4S6V9KZKX4K9TQ7M"
}
```

`message` không được chứa stack trace, secret hoặc thông tin hạ tầng nhạy cảm.

| HTTP Status | Khi dùng |
| :--- | :--- |
| `400` | Request body/query parameter sai format hoặc validation lỗi. |
| `401` | Thiếu hoặc sai authentication credential. |
| `403` | User đã xác thực nhưng không có quyền. |
| `404` | Resource không tồn tại hoặc user không được nhìn thấy. |
| `413` | Request vượt giới hạn body size. |
| `429` | Vượt rate limit. |
| `503` | Dependency tạm thời không khả dụng. |

## 4. Control API Core Contracts

### 4.1. Resource Hierarchy

```text
Workspace
-> Project
-> Trace Application
-> API Key
```

Search log luôn nằm trong scope `workspaceId + projectId`. Application và environment là filter/context bổ sung, không thay thế project scope.

### 4.2. API Key Creation

```http
POST /api/workspaces/{workspaceId}/projects/{projectId}/applications/{applicationId}/api-keys
Authorization: Bearer {accessToken}
Content-Type: application/json
```

Request:

```json
{
  "name": "production-ingestion-key",
  "environment": "production",
  "expiresAt": "2026-12-18T00:00:00Z"
}
```

Response:

```json
{
  "apiKeyId": "01J8Z7Y3YF4S6V9KZKX4K9TQ7M",
  "name": "production-ingestion-key",
  "environment": "production",
  "keyPrefix": "tfk_prod_01J8",
  "secret": "tfk_prod_01J8.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "status": "active",
  "expiresAt": "2026-12-18T00:00:00Z",
  "createdAt": "2026-09-19T00:00:00Z"
}
```

`secret` chỉ được trả về một lần khi tạo. Các endpoint sau đó chỉ trả metadata và `keyPrefix`.

### 4.3. API Key List

```http
GET /api/workspaces/{workspaceId}/projects/{projectId}/applications/{applicationId}/api-keys
Authorization: Bearer {accessToken}
```

Response:

```json
{
  "items": [
    {
      "apiKeyId": "01J8Z7Y3YF4S6V9KZKX4K9TQ7M",
      "name": "production-ingestion-key",
      "environment": "production",
      "keyPrefix": "tfk_prod_01J8",
      "status": "active",
      "expiresAt": "2026-12-18T00:00:00Z",
      "lastUsedAt": "2026-09-19T10:15:30Z",
      "createdAt": "2026-09-19T00:00:00Z"
    }
  ]
}
```

### 4.4. API Key Revoke

```http
DELETE /api/workspaces/{workspaceId}/projects/{projectId}/applications/{applicationId}/api-keys/{apiKeyId}
Authorization: Bearer {accessToken}
```

Response:

```json
{
  "apiKeyId": "01J8Z7Y3YF4S6V9KZKX4K9TQ7M",
  "status": "revoked",
  "revokedAt": "2026-09-19T11:00:00Z"
}
```

Revoke phải làm invalid cache liên quan trong Redis hoặc dựa vào TTL đủ ngắn để tránh key revoked tiếp tục được dùng quá lâu. Chi tiết policy thuộc Reliability Design.

## 5. Internal API Key Validation Contract

Ingestion API không tự đọc PostgreSQL và không tự suy luận tenant context. Nó validate API key thông qua Control API hoặc dùng Redis cache hợp lệ theo policy.

```http
POST /internal/api-keys/validate
X-Internal-Secret: {internalSecret}
Content-Type: application/json
```

Request:

```json
{
  "apiKey": "tfk_prod_01J8.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
}
```

Valid response:

```json
{
  "valid": true,
  "apiKeyId": "01J8Z7Y3YF4S6V9KZKX4K9TQ7M",
  "workspaceId": "01J8Z7A6KQ1X4R91YPXZR3ANH2",
  "projectId": "01J8Z7BAW6EMX0QK9W0K4D2S7R",
  "applicationId": "01J8Z7CC9P0NZV8J6SP3Z8BW2V",
  "environment": "production"
}
```

Invalid response:

```json
{
  "valid": false
}
```

Control API trả `valid: false` nếu API key sai format, không tồn tại, hash không khớp, revoked, expired hoặc thuộc resource archived/deleted.

## 6. Redis Contract

Redis là core supporting infrastructure, nhưng contract của Redis phải giữ nhỏ để không biến Redis thành source of truth.

| Use case | Key direction | Value | TTL |
| :--- | :--- | :--- | :--- |
| API key validation cache | `api_key_validation:{keyPrefix}:{secretHashFingerprint}` | Tenant context tối thiểu và `apiKeyId`. | Ngắn, ví dụ 30-300 giây. |
| Rate limit counter | `rate_limit:{apiKeyId}:{window}` | Counter request/log trong time window. | Theo window. |
| Short usage counter | `usage:{projectId}:{window}` | Counter ngắn hạn phục vụ quota nếu bật. | Theo window. |

Redis value không được chứa full API key secret. Nếu cache miss hoặc Redis lỗi, hệ thống phải có đường fallback gọi Control API validation trực tiếp, trừ khi rate limit được cấu hình fail-closed trong Reliability Design.

## 7. Ingestion API Contracts

### 7.1. Single Log

```http
POST /logs
Authorization: ApiKey {secret}
Content-Type: application/json
```

Request:

```json
{
  "timestamp": "2026-09-19T10:15:30Z",
  "service": "checkout-api",
  "level": "ERROR",
  "message": "Payment provider timeout",
  "traceId": "trace-7f9c2a",
  "correlationId": "order-10001",
  "metadata": {
    "orderId": "10001",
    "provider": "stripe",
    "durationMs": 3500
  }
}
```

Accepted response:

```json
{
  "status": "accepted"
}
```

### 7.2. Batch Logs

```http
POST /logs/batch
Authorization: ApiKey {secret}
Content-Type: application/json
```

Request:

```json
{
  "logs": [
    {
      "timestamp": "2026-09-19T10:15:30Z",
      "service": "checkout-api",
      "level": "INFO",
      "message": "Payment request started",
      "traceId": "trace-7f9c2a",
      "correlationId": "order-10001",
      "metadata": {
        "orderId": "10001"
      }
    }
  ]
}
```

Accepted response:

```json
{
  "status": "accepted",
  "acceptedLogs": 1
}
```

### 7.3. Ingestion Field Rules

| Field | Required | Rule |
| :--- | :---: | :--- |
| `timestamp` | No | Nếu có, phải là RFC3339; nếu thiếu dùng thời điểm nhận request. |
| `service` | Yes | Không được rỗng. |
| `level` | Yes | Một trong `DEBUG`, `INFO`, `WARN`, `ERROR`, `FATAL`. |
| `message` | Yes | Không được rỗng. |
| `traceId` | No | Dùng để truy vết request xuyên service. |
| `correlationId` | No | Dùng để nhóm log theo business/request context. |
| `metadata` | No | JSON object hợp lệ trong giới hạn size/depth/field count. |

Client không được quyết định `workspaceId`, `projectId`, `applicationId` hoặc `environment`.

## 8. Kafka Event Contract

| Contract item | Value |
| :--- | :--- |
| Main topic | `traceflow.logs` |
| Producer | Ingestion API |
| Consumer | Log Processor |
| Encoding | JSON |
| Event ownership | Ingestion API sinh `eventId` và tenant context |

```json
{
  "schemaVersion": 1,
  "eventId": "01J8Z7Y3YF4S6V9KZKX4K9TQ7M",
  "timestamp": "2026-09-19T10:15:30Z",
  "receivedAt": "2026-09-19T10:15:31Z",
  "workspaceId": "01J8Z7A6KQ1X4R91YPXZR3ANH2",
  "projectId": "01J8Z7BAW6EMX0QK9W0K4D2S7R",
  "applicationId": "01J8Z7CC9P0NZV8J6SP3Z8BW2V",
  "environment": "production",
  "service": "checkout-api",
  "level": "ERROR",
  "message": "Payment provider timeout",
  "traceId": "trace-7f9c2a",
  "correlationId": "order-10001",
  "metadata": {
    "provider": "stripe",
    "durationMs": 3500
  }
}
```

Kafka partition key sẽ được chốt trong Reliability Design. Contract chỉ yêu cầu event có đủ tenant context và `eventId`.

## 9. OpenSearch Document Contract

OpenSearch document được tạo từ enriched Kafka event sau khi processor validate/normalize.

| Contract item | Value |
| :--- | :--- |
| Index name | `traceflow-logs` trong local/default config |
| Writer | Log Processor |
| Reader | Control API Search |
| Query access | Chỉ thông qua Control API |

Document schema giữ cùng core fields với Kafka event. Mapping direction:

| Field | Mapping direction | Query usage |
| :--- | :--- | :--- |
| `eventId` | keyword | Lookup/debug. |
| `workspaceId` | keyword | Required tenant filter. |
| `projectId` | keyword | Required tenant filter. |
| `applicationId` | keyword | Optional filter. |
| `environment` | keyword | Optional filter. |
| `service` | keyword + text nếu cần | Filter/search. |
| `level` | keyword | Optional filter. |
| `timestamp` | date | Sort và range query. |
| `receivedAt` | date | Debug latency. |
| `traceId` | keyword | Trace lookup. |
| `correlationId` | keyword | Correlation lookup. |
| `message` | text | Full-text search. |
| `metadata` | object/flattened | Extension data trong guardrails. |

## 10. Search API Contract

```http
GET /api/workspaces/{workspaceId}/projects/{projectId}/logs
Authorization: Bearer {accessToken}
```

Query parameters:

| Parameter | Required | Meaning |
| :--- | :---: | :--- |
| `from` | No | Start timestamp. |
| `to` | No | End timestamp. |
| `applicationId` | No | Filter application. |
| `environment` | No | Filter environment. |
| `level` | No | Filter log level. |
| `service` | No | Filter service. |
| `traceId` | No | Trace lookup. |
| `correlationId` | No | Correlation lookup. |
| `q` | No | Message full-text query. |
| `limit` | No | Page size with max limit. |
| `cursor` | No | Pagination cursor. |

Response:

```json
{
  "items": [
    {
      "eventId": "01J8Z7Y3YF4S6V9KZKX4K9TQ7M",
      "timestamp": "2026-09-19T10:15:30Z",
      "workspaceId": "01J8Z7A6KQ1X4R91YPXZR3ANH2",
      "projectId": "01J8Z7BAW6EMX0QK9W0K4D2S7R",
      "applicationId": "01J8Z7CC9P0NZV8J6SP3Z8BW2V",
      "environment": "production",
      "service": "checkout-api",
      "level": "ERROR",
      "message": "Payment provider timeout",
      "traceId": "trace-7f9c2a",
      "correlationId": "order-10001",
      "metadata": {
        "provider": "stripe"
      }
    }
  ],
  "nextCursor": null
}
```

Control API phải kiểm tra project access trước khi query OpenSearch và luôn inject tenant filter.

## 11. DLQ Event Contract

| Contract item | Value |
| :--- | :--- |
| Topic | `traceflow.logs.dlq` |
| Producer | Log Processor |
| Consumer | Debug/replay tooling trong tương lai |
| Encoding | JSON |

```json
{
  "dlqId": "01J8Z9ABCDEFGHJKMNPQRSTUV",
  "failedAt": "2026-09-19T10:16:00Z",
  "failureStage": "indexing",
  "failureReason": "OpenSearch bulk item failed with status 400",
  "sourceTopic": "traceflow.logs",
  "sourcePartition": 2,
  "sourceOffset": 1842,
  "eventId": "01J8Z7Y3YF4S6V9KZKX4K9TQ7M",
  "workspaceId": "01J8Z7A6KQ1X4R91YPXZR3ANH2",
  "projectId": "01J8Z7BAW6EMX0QK9W0K4D2S7R",
  "payload": {
    "original": "event or raw payload"
  }
}
```

`failureStage` tối thiểu gồm `json_decode`, `validation` và `indexing`.

## 12. Bulk Index Result Contract

Log Processor cần một internal result model để phân biệt full failure và partial failure.

| Field | Meaning |
| :--- | :--- |
| `attempted` | Số item gửi vào bulk request. |
| `indexed` | Số item index thành công. |
| `failed` | Số item lỗi. |
| `items` | Danh sách item-level result khi OpenSearch trả partial failure. |

Full bulk failure nghĩa là request bulk không có response item-level đáng tin cậy. Partial failure nghĩa là bulk request thành công ở mức HTTP nhưng một số item lỗi.

## 13. Nice-To-Have Contract Boundaries

| Capability | Contract Direction |
| :--- | :--- |
| Retention | API/config đơn giản cho fixed retention values; không archive/restore contract. |
| Quota | Rate/volume limit response dùng `429`; Redis counter là implementation detail. |
| Operational Insights | Summary endpoint đọc từ OpenSearch qua Control API. |
| Benchmark | Test output format trong Testing Strategy, không ảnh hưởng runtime API. |
| Minimal Web UI | Chỉ consume public Control API và Ingestion examples. |

Alerting, backup/restore và advanced dashboard không có contract bắt buộc trong core.
