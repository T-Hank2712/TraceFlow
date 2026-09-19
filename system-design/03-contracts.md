# Contracts

## 1. Mục Tiêu Tài Liệu

Tài liệu này định nghĩa các contract chính giữa những thành phần của TraceFlow. Contract là phần giúp các service giao tiếp với nhau bằng định dạng rõ ràng, ổn định và có thể kiểm thử được.

Nếu Requirements trả lời hệ thống cần làm gì, HLD trả lời hệ thống gồm những thành phần nào, thì Contracts trả lời:

```text
Các thành phần trao đổi dữ liệu với nhau bằng format nào?
Field nào bắt buộc?
Field nào optional?
Khi lỗi xảy ra thì response hoặc event trông như thế nào?
Khi schema thay đổi thì service nào bị ảnh hưởng?
```

Tài liệu này chưa đi sâu vào class, function hoặc module nội bộ. Các chi tiết đó thuộc Low-Level Design.

## 2. Contract Boundaries

TraceFlow có nhiều loại giao tiếp khác nhau. Mỗi loại có mức độ ổn định và yêu cầu compatibility khác nhau.

| Boundary | Producer | Consumer | Contract chính |
| :--- | :--- | :--- | :--- |
| Public Control API | Control API | Web UI, API client, user-facing consumers | JSON HTTP API, JWT authentication, error response |
| Internal Validation API | Control API | Ingestion API | Internal JSON HTTP API, service-to-service authentication |
| Public Ingestion API | Ingestion API | Client Application | JSON HTTP API, API key authentication |
| Kafka Log Event | Ingestion API | Log Processor | Enriched log event schema |
| OpenSearch Document | Log Processor | Control API Search | Indexed log document schema |
| DLQ Event | Log Processor | Operations/debug/replay tooling | Dead letter event schema |

Public contracts phải ổn định hơn internal contracts vì chúng có thể được dùng bởi client hoặc UI. Internal contracts vẫn cần rõ ràng vì nếu lệch schema, pipeline sẽ hỏng ở runtime.

## 3. API Conventions

TraceFlow dùng JSON cho HTTP request và response. Field name trong JSON sử dụng `camelCase` để thống nhất giữa Control API, Ingestion API và các event đi qua Kafka.

Thời gian được biểu diễn bằng chuỗi RFC3339 theo UTC. Ví dụ:

```json
"2026-09-19T10:15:30Z"
```

ID của resource sử dụng định dạng ULID ở dạng string. Các field như `workspaceId`, `projectId`, `applicationId`, `apiKeyId` và `eventId` đều được truyền qua API hoặc event dưới dạng string.

### 3.1. Authentication Convention

TraceFlow có ba loại authentication boundary:

| Boundary | Header | Ý nghĩa |
| :--- | :--- | :--- |
| User-facing Control API | `Authorization: Bearer {accessToken}` | User đã đăng nhập gọi API quản trị hoặc search. |
| Public Ingestion API | `Authorization: ApiKey {secret}` | Client Application gửi log bằng API key. |
| Internal service-to-service API | `X-Internal-Secret: {secret}` | Ingestion API gọi internal endpoint của Control API. |

JWT không được dùng để gửi log thay cho application. API key không được dùng để gọi API quản trị hoặc search.

### 3.2. Standard Success Response

Các command API có thể trả response theo resource cụ thể. Tuy nhiên với các thao tác đơn giản, response nên giữ cấu trúc ngắn gọn:

```json
{
  "status": "accepted"
}
```

Search và list API trả về data kèm pagination metadata. Chi tiết nằm ở phần Log Search Contract.

### 3.3. Standard Error Response

Error response cần thống nhất để UI, API client và test có thể xử lý lỗi nhất quán.

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

`code` là mã lỗi ổn định cho client xử lý. `message` là mô tả ngắn gọn, không chứa stack trace hoặc secret. `details` dùng cho validation error hoặc batch error. `requestId` giúp truy vết log nội bộ.

Các mã lỗi HTTP nên dùng nhất quán:

| HTTP status | Khi dùng |
| :--- | :--- |
| `400 Bad Request` | Request body sai format, validation lỗi hoặc query parameter không hợp lệ. |
| `401 Unauthorized` | Thiếu hoặc sai authentication credential. |
| `403 Forbidden` | User đã xác thực nhưng không có quyền trên resource. |
| `404 Not Found` | Resource không tồn tại hoặc user không có quyền nhìn thấy resource đó. |
| `409 Conflict` | Trạng thái hiện tại của resource không cho phép thao tác. |
| `413 Payload Too Large` | Request vượt giới hạn body size. |
| `429 Too Many Requests` | Vượt rate limit. |
| `503 Service Unavailable` | Dependency như Kafka, OpenSearch hoặc internal service tạm thời không khả dụng. |

## 4. Control API Contracts

Control API là user-facing API cho quản trị resource, API key lifecycle và log search. Tài liệu này không liệt kê đầy đủ mọi endpoint CRUD, mà chốt những contract ảnh hưởng trực tiếp tới pipeline và service boundary.

### 4.1. Resource Hierarchy

TraceFlow tổ chức resource theo hierarchy:

```text
Workspace
-> Project
-> Trace Application
-> API Key
```

Search log luôn nằm trong scope:

```text
Workspace + Project
```

Trace Application và environment là filter hoặc context bổ sung, không thay thế project scope.

### 4.2. API Key Creation Contract

Project Manager tạo API key cho một trace application. Full secret chỉ được trả về một lần trong response tạo key.

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
  "expirationPolicy": "90_days"
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

`secret` không được lưu plaintext và không được trả lại ở bất kỳ endpoint nào khác. Các list/detail endpoint chỉ được trả `keyPrefix`, metadata và status.

### 4.3. API Key List Contract

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

### 4.4. API Key Revoke Contract

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

Nếu hệ thống sử dụng Redis hoặc cache tương đương trong giai đoạn sau, revoke API key phải làm mất hiệu lực cache liên quan theo policy được chốt trong Reliability Design.

## 5. Internal API Key Validation Contract

Internal API key validation là contract quan trọng nhất giữa Ingestion API và Control API.

Ingestion API không tự đọc PostgreSQL và không tự suy luận tenant context. Nó gửi API key sang Control API để validate. Control API là source of truth cho API key status, expiration, resource status và tenant context.

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
  "workspaceId": "01J8Z7A6KQ1X4R91YPXZR3ANH2",
  "projectId": "01J8Z7BAW6EMX0QK9W0K4D2S7R",
  "applicationId": "01J8Z7CC9P0NZV8J6SP3Z8BW2V",
  "environment": "production"
}
```

Invalid response:

```json
{
  "valid": false,
  "workspaceId": null,
  "projectId": null,
  "applicationId": null,
  "environment": null
}
```

Internal validation endpoint không được trả API key secret, secret hash hoặc thông tin database nội bộ.

### 5.1. Validation Rules

Control API trả `valid: false` nếu API key rơi vào một trong các trường hợp sau:

| Trường hợp | Kết quả |
| :--- | :--- |
| Sai format | Invalid |
| Không tồn tại | Invalid |
| Hash không khớp | Invalid |
| API key đã revoked | Invalid |
| API key đã expired | Invalid |
| Trace application bị archived/deleted | Invalid |
| Project bị archived/deleted | Invalid |
| Workspace bị archived/deleted | Invalid |

Khi API key hợp lệ, Control API có thể cập nhật `lastUsedAt` theo policy hiện tại. Nếu việc cập nhật `lastUsedAt` thất bại do lỗi tạm thời, behavior chi tiết sẽ được chốt trong Reliability Design để tránh làm ingestion path quá dễ lỗi.

## 6. Ingestion API Contracts

Ingestion API là public API dành cho Client Application gửi log.

Authentication dùng API key:

```http
Authorization: ApiKey {secret}
```

Ingestion API chỉ trả success sau khi log event được publish thành công vào Kafka. Response success không có nghĩa là log đã được index vào OpenSearch hoặc đã search được.

### 6.1. Single Log Contract

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

### 6.2. Batch Logs Contract

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
    },
    {
      "timestamp": "2026-09-19T10:15:32Z",
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
  ]
}
```

Accepted response:

```json
{
  "status": "accepted",
  "acceptedLogs": 2
}
```

### 6.3. Ingestion Field Rules

| Field | Required | Rule |
| :--- | :---: | :--- |
| `timestamp` | No | Nếu có, phải là RFC3339. Nếu thiếu, Ingestion API dùng thời điểm nhận request. |
| `service` | Yes | Không được rỗng. Đại diện cho service sinh log. |
| `level` | Yes | Một trong `DEBUG`, `INFO`, `WARN`, `ERROR`, `FATAL`. |
| `message` | Yes | Không được rỗng. |
| `traceId` | No | Dùng để truy vết request xuyên nhiều service. |
| `correlationId` | No | Dùng để nhóm log theo business/request context. |
| `metadata` | No | Chỉ chứa JSON object hợp lệ trong giới hạn size, depth và field count. |

Client Application không được truyền `workspaceId`, `projectId`, `applicationId` hoặc `environment` để quyết định tenant context. Nếu các field này xuất hiện trong request, Ingestion API không được dùng chúng làm source of truth.

### 6.4. Ingestion Validation Error

```json
{
  "code": "validation_error",
  "message": "Invalid batch log payload.",
  "details": [
    {
      "index": 1,
      "field": "level",
      "message": "Level must be one of DEBUG, INFO, WARN, ERROR, FATAL."
    }
  ],
  "requestId": "01J8Z7Y3YF4S6V9KZKX4K9TQ7M"
}
```

Batch validation nên trả được `index` để client biết item nào lỗi.

## 7. Kafka Event Contract

Kafka main topic lưu enriched log event đã được Ingestion API validate và enrich bằng tenant context.

| Contract item | Giá trị |
| :--- | :--- |
| Main topic | `traceflow.logs` |
| Producer | Ingestion API |
| Consumer | Log Processor |
| Encoding | JSON |
| Event ownership | Ingestion API sinh `eventId` và tenant context |

### 7.1. Enriched Log Event Schema

```json
{
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

### 7.2. Kafka Event Field Contract

| Field | Required | Producer | Notes |
| :--- | :---: | :--- | :--- |
| `eventId` | Yes | Ingestion API | Unique ID do Ingestion API sinh. |
| `timestamp` | Yes | Client/Ingestion API | Client gửi RFC3339 hoặc Ingestion API dùng thời điểm nhận request. |
| `receivedAt` | Yes | Ingestion API | Thời điểm hệ thống nhận log. |
| `workspaceId` | Yes | Ingestion API | Lấy từ internal API key validation response. |
| `projectId` | Yes | Ingestion API | Lấy từ internal API key validation response. |
| `applicationId` | Yes | Ingestion API | Lấy từ internal API key validation response. |
| `environment` | Yes | Ingestion API | Lấy từ API key metadata. |
| `service` | Yes | Client Application | Đã trim/normalize khi cần. |
| `level` | Yes | Client Application | Normalize uppercase. |
| `message` | Yes | Client Application | Không được rỗng. |
| `traceId` | No | Client Application | Optional. |
| `correlationId` | No | Client Application | Optional. |
| `metadata` | No | Client Application | Phải nằm trong guardrails đã chốt. |

Kafka partition key sẽ được chốt trong Reliability Design. HLD hiện chỉ yêu cầu partitioning strategy phải hỗ trợ throughput, ordering hợp lý và consumer group scaling.

## 8. OpenSearch Document Contract

OpenSearch document được tạo từ enriched log event sau khi Log Processor validate và normalize.

| Contract item | Giá trị |
| :--- | :--- |
| Index name | `traceflow-logs` trong local/default config |
| Writer | Log Processor |
| Reader | Control API Search |
| Query access | Chỉ thông qua Control API |

### 8.1. Log Document Schema

```json
{
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

### 8.2. Mapping Direction

OpenSearch mapping phải phục vụ hai nhu cầu chính: exact filter theo tenant fields và time-based search theo timestamp.

| Field | Mapping direction | Query usage |
| :--- | :--- | :--- |
| `eventId` | keyword | Lookup/debug. |
| `workspaceId` | keyword | Required tenant filter. |
| `projectId` | keyword | Required tenant filter. |
| `applicationId` | keyword | Optional filter. |
| `environment` | keyword | Optional filter. |
| `service` | keyword + searchable text nếu cần | Optional filter/search. |
| `level` | keyword | Optional filter. |
| `timestamp` | date | Sort và range query. |
| `receivedAt` | date | Debug latency và ingestion timing. |
| `traceId` | keyword | Trace lookup. |
| `correlationId` | keyword | Correlation lookup. |
| `message` | text | Full-text search nếu được hỗ trợ. |
| `metadata` | object/flattened direction | Lưu thông tin bổ sung trong guardrails. |

Control API Search luôn filter theo `workspaceId` và `projectId` trước khi áp dụng các filter khác.

## 9. Log Search Contract

Search API thuộc Control API và yêu cầu JWT authentication.

```http
GET /api/workspaces/{workspaceId}/projects/{projectId}/logs
Authorization: Bearer {accessToken}
```

Query parameters:

| Parameter | Required | Notes |
| :--- | :---: | :--- |
| `applicationId` | No | Filter theo trace application. |
| `environment` | No | Filter theo environment. |
| `level` | No | Filter theo log level. |
| `service` | No | Filter theo service. |
| `traceId` | No | Filter theo trace ID. |
| `correlationId` | No | Filter theo correlation ID. |
| `from` | No | RFC3339 start time. |
| `to` | No | RFC3339 end time. |
| `page` | No | Default `1`. |
| `pageSize` | No | Default và max được chốt trong implementation. |

Response:

```json
{
  "items": [
    {
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
  ],
  "total": 1,
  "page": 1,
  "pageSize": 50
}
```

Nếu request không truyền `from` và `to`, Search API phải áp dụng default time range hợp lý. Requirements đã yêu cầu có default time range; giá trị cụ thể có thể được chốt trong LLD hoặc implementation.

## 10. DLQ Event Contract

DLQ event được publish khi Log Processor không thể xử lý message thành công.

| Contract item | Giá trị |
| :--- | :--- |
| DLQ topic | `traceflow.logs.dlq` |
| Producer | Log Processor |
| Consumer | Operations/debug/replay tooling trong tương lai |
| Encoding | JSON |

### 10.1. DLQ Event Schema

```json
{
  "failedAt": "2026-09-19T10:16:00Z",
  "sourceTopic": "traceflow.logs",
  "partition": 0,
  "offset": 128,
  "failureStage": "indexing",
  "failureReason": "OpenSearch bulk item failed: mapper_parsing_exception",
  "originalPayload": "{...}",
  "eventId": "01J8Z7Y3YF4S6V9KZKX4K9TQ7M",
  "workspaceId": "01J8Z7A6KQ1X4R91YPXZR3ANH2",
  "projectId": "01J8Z7BAW6EMX0QK9W0K4D2S7R",
  "applicationId": "01J8Z7CC9P0NZV8J6SP3Z8BW2V"
}
```

### 10.2. Failure Stage

| failureStage | Khi dùng |
| :--- | :--- |
| `json_decode` | Kafka message không parse được thành JSON. |
| `validation` | Message parse được nhưng thiếu hoặc sai field bắt buộc. |
| `indexing` | Event hợp lệ nhưng thất bại khi index vào OpenSearch. |

DLQ event phải giữ `originalPayload` để hỗ trợ debug hoặc replay thủ công trong tương lai. Nếu message không parse được, các field như `eventId`, `workspaceId`, `projectId` và `applicationId` có thể rỗng.

## 11. Bulk Indexing Result Contract

OpenSearch Bulk API có thể trả về ba nhóm kết quả:

| Kết quả | Ý nghĩa | Processor behavior |
| :--- | :--- | :--- |
| Success | Bulk request thành công và tất cả item index thành công. | Log batch result, không gửi DLQ. |
| Full failure | Bulk request thất bại toàn bộ do network, timeout, auth hoặc OpenSearch unavailable. | Retry theo policy; nếu hết retry thì DLQ toàn bộ batch. |
| Partial failure | Bulk request thành công nhưng một số item thất bại. | Chỉ DLQ các item failed; không DLQ item đã index thành công. |

Contract nội bộ giữa repository và processor nên biểu diễn partial failure bằng danh sách index của item lỗi trong batch. Ví dụ:

```json
{
  "failedItems": [
    {
      "index": 2,
      "reason": "mapper_parsing_exception: failed to parse field metadata.durationMs"
    }
  ]
}
```

`index` phải tương ứng với vị trí của item trong batch ban đầu để Log Processor gửi đúng Kafka message context vào DLQ.

## 12. Contract Versioning Và Compatibility

TraceFlow chưa cần versioning phức tạp trong giai đoạn đầu, nhưng contract thay đổi phải tuân theo một số nguyên tắc để tránh làm hỏng pipeline.

Thay đổi backward-compatible:

| Thay đổi | Điều kiện |
| :--- | :--- |
| Thêm optional field vào request/event/document | Consumer bỏ qua field chưa biết hoặc xử lý default. |
| Thêm optional filter vào Search API | Không thay đổi behavior của request cũ. |
| Thêm error code mới | Client vẫn xử lý được fallback message. |

Thay đổi breaking:

| Thay đổi | Tác động |
| :--- | :--- |
| Đổi tên field bắt buộc | Producer/consumer lệch schema. |
| Xóa field đang được consumer dùng | Có thể làm decode hoặc query fail. |
| Đổi kiểu dữ liệu của field | Có thể làm OpenSearch mapping lỗi hoặc client decode lỗi. |
| Đổi ý nghĩa response success | Có thể làm client hiểu sai trạng thái pipeline. |

Khi thay đổi Kafka event schema, OpenSearch document schema hoặc DLQ schema, cần kiểm tra ít nhất các thành phần sau:

| Contract thay đổi | Thành phần cần kiểm tra |
| :--- | :--- |
| Ingestion request | Client Application, Ingestion API validation, error response. |
| Internal API key validation | Control API, Ingestion API adapter. |
| Kafka event | Ingestion API mapper, Log Processor model, DLQ payload handling. |
| OpenSearch document | Log Processor repository, OpenSearch mapping, Control API Search reader. |
| DLQ event | Log Processor DLQ producer, operations/debug tooling. |

## 13. Contract Ownership

Mỗi contract cần có producer và consumer rõ ràng để tránh thay đổi một phía làm hỏng phía còn lại.

| Contract | Owner chính | Consumer chính |
| :--- | :--- | :--- |
| Public Control API | Control API | Web UI, user-facing API clients |
| Internal API key validation | Control API | Ingestion API |
| Ingestion API | Ingestion API | Client Applications |
| Kafka log event | Ingestion API | Log Processor |
| OpenSearch log document | Log Processor | Control API Search |
| DLQ event | Log Processor | Operations/debug/replay tooling |

Nguyên tắc chung là producer không được thay đổi field bắt buộc hoặc kiểu dữ liệu mà không cập nhật consumer tương ứng. Những thay đổi liên quan tới schema phải được phản ánh trong Contracts, LLD và Testing Strategy.
