# Low-Level Design

## 1. Mục Tiêu Tài Liệu

Low-Level Design mô tả thiết kế chi tiết cho TraceFlow Core:

```text
Multi-tenant log ingestion and search pipeline
```

Tài liệu này không kể lại code hiện tại và không mô tả tiến độ đã làm. Mục tiêu của LLD là chốt cách hệ thống nên được tổ chức ở mức module, model, interface, state, flow xử lý và failure semantics để khi implementation bắt đầu, từng service đều có hướng đi rõ ràng.

LLD chỉ thiết kế core scope: Auth/RBAC, Workspace/Project/Application, API Key, Ingestion API, Kafka, Log Processor, OpenSearch, Batch + Bulk Indexing, Retry + DLQ, Search API và Redis. Retention, quota, operational insights, benchmark và minimal Web UI là extension nhỏ. Alerting, backup/restore và advanced dashboard không thuộc core LLD.

## 2. Design Boundaries

TraceFlow có ba runtime boundary chính. Mỗi boundary có quyền sở hữu dữ liệu và trách nhiệm riêng, tránh việc service này xử lý logic thuộc service khác.

| Boundary | Owns | Must Not Do |
| :--- | :--- | :--- |
| Control API | User identity, workspace, project, application, RBAC, API key metadata/hash, search authorization. | Không nhận high-throughput log ingestion trực tiếp. |
| Ingestion API | Request validation, API key validation call/cache, rate limit, tenant enrichment, Kafka publish. | Không lưu business metadata, không index OpenSearch. |
| Log Processor | Kafka consume, internal event validation, batching, bulk indexing, retry, DLQ. | Không gọi PostgreSQL để resolve tenant context. |

PostgreSQL là source of truth cho control metadata. Redis là cache/rate-limit/counter ngắn hạn. Kafka là event stream. OpenSearch là search storage. Không storage nào được dùng thay vai trò của storage khác.

## 3. Core Domain Model

Domain model của TraceFlow xoay quanh tenant hierarchy và log ownership.

```text
User
  └── WorkspaceMembership

Workspace
  └── Project
        ├── ProjectMembership
        └── TraceApplication
              └── ApiKey

ApiKey
  └── resolves to TenantContext

TenantContext
  └── attached to LogEvent
```

| Model | Key Fields | Design Meaning |
| :--- | :--- | :--- |
| `Workspace` | `id`, `name`, `status` | Tenant boundary cấp cao nhất. |
| `Project` | `id`, `workspaceId`, `name`, `status` | Scope chính cho log ownership, RBAC và search. |
| `TraceApplication` | `id`, `projectId`, `name`, `status` | Workload gửi log vào TraceFlow. |
| `ApiKey` | `id`, `applicationId`, `prefix`, `secretHash`, `status`, `expiresAt` | Credential ingestion; full secret không được lưu. |
| `TenantContext` | `workspaceId`, `projectId`, `applicationId`, `environment`, `apiKeyId` | Context server-side gắn vào mọi log event. |
| `LogEvent` | `eventId`, `tenantContext`, `timestamp`, `service`, `level`, `message`, `metadata` | Internal event đi qua Kafka và OpenSearch. |

`TenantContext` không được nhận từ client request. Nó chỉ được tạo sau khi API key hợp lệ được validate bởi Control API hoặc cache Redis hợp lệ theo policy.

## 4. State Models

### 4.1. Resource State

Workspace, Project và TraceApplication cần state đủ đơn giản để bảo vệ ingestion và search.

| State | Meaning | API Key Validation |
| :--- | :--- | :--- |
| `active` | Resource đang dùng bình thường. | Có thể hợp lệ nếu API key cũng hợp lệ. |
| `archived` | Resource bị ngừng sử dụng nhưng còn dữ liệu. | API key bị từ chối. |
| `deleted` | Resource không còn được dùng trong hệ thống. | API key bị từ chối. |

Search chỉ trả dữ liệu nếu user có quyền với project. Việc project/application archived có cho search lịch sử hay không sẽ do business policy ở Control API quyết định, nhưng ingestion mới phải bị chặn.

### 4.2. API Key State

```text
active -> revoked
active -> expired
```

| State | Meaning | Ingestion Behavior |
| :--- | :--- | :--- |
| `active` | Key còn hiệu lực và resource chain active. | Cho phép nếu hash khớp và chưa hết hạn. |
| `revoked` | User/admin đã thu hồi key. | Từ chối ngay. |
| `expired` | Key đã quá thời hạn. | Từ chối ngay. |

Nếu Redis đang cache validation result, revoke phải invalidate cache khi có thể. TTL vẫn cần đủ ngắn để giảm rủi ro stale cache.

### 4.3. Log Event Processing State

```text
received
-> published
-> consumed
-> buffered
-> indexed

received/published/consumed
-> failed
-> dlq
```

| State | Owner | Meaning |
| :--- | :--- | :--- |
| `received` | Ingestion API | Request đã vào service nhưng chưa chắc vào Kafka. |
| `published` | Ingestion API/Kafka | Event đã được Kafka accept. |
| `consumed` | Log Processor | Processor đã nhận Kafka message. |
| `buffered` | Log Processor | Event hợp lệ đang nằm trong batch buffer. |
| `indexed` | Log Processor/OpenSearch | Event đã được OpenSearch index thành công. |
| `dlq` | Log Processor/Kafka | Event không xử lý được và đã có DLQ record. |

Ingestion response success chỉ tương ứng với `published`, không tương ứng với `indexed`.

## 5. Control API Design

Control API là nơi chốt correctness của user access, resource ownership và API key lifecycle.

### 5.1. Internal Modules

| Module | Responsibility | Output |
| :--- | :--- | :--- |
| Auth | Login, refresh, logout, JWT issuing. | Authenticated user context. |
| Access Control | Workspace/project permission checks. | Allow/deny decision. |
| Resource Management | Workspace, project, trace application lifecycle. | Resource metadata. |
| API Key Management | Create/list/revoke/validate API key. | API key metadata hoặc tenant context. |
| Search | Build scoped OpenSearch query. | Search result page. |
| Redis Cache Adapter | Validation cache invalidation/read/write. | Cached tenant context hoặc miss. |

### 5.2. Access Control Contract

Access control should answer one question only:

```text
Can this user perform this action on this workspace/project/application?
```

| Action | Required Check |
| :--- | :--- |
| Create project | User can manage workspace. |
| Create application | User can manage project. |
| Create/revoke API key | User can manage application/project. |
| Search logs | User can view project logs. |

Handlers không tự viết lại role logic. Chúng gọi access service để giữ rule nhất quán.

### 5.3. API Key Validation Algorithm

```text
Input: full API key secret

1. Parse key prefix and secret material.
2. Try Redis validation cache by safe fingerprint.
3. If cache hit and not expired, return tenant context.
4. If cache miss, load API key by prefix from PostgreSQL.
5. Verify secret hash.
6. Check API key state: active, not revoked, not expired.
7. Check Workspace/Project/Application state: active.
8. Build TenantContext.
9. Store TenantContext in Redis with short TTL.
10. Return TenantContext.
```

Redis value must not contain full API key secret or secret hash. Cache key must avoid storing raw secret.

### 5.4. Search Query Construction

Search query construction follows a strict order:

```text
1. Authenticate user.
2. Check project access.
3. Start query with mandatory tenant filters:
   - workspaceId
   - projectId
4. Add optional filters:
   - applicationId
   - environment
   - level
   - service
   - traceId
   - correlationId
   - time range
   - full-text query
5. Apply pagination and sort.
6. Execute OpenSearch query.
```

Optional filters are never allowed to replace tenant filters.

## 6. Ingestion API Design

Ingestion API is optimized for short request path and predictable failure behavior.

### 6.1. Internal Modules

| Module | Responsibility |
| :--- | :--- |
| HTTP Transport | Parse headers/body, apply body size limit, map errors to HTTP responses. |
| API Key Validator | Validate key through Redis/Control API and return tenant context. |
| Rate Limiter | Use Redis counters to protect ingestion path. |
| Payload Validator | Validate single/batch log payload. |
| Event Builder | Create enriched Kafka event from request + tenant context. |
| Kafka Publisher | Publish event to Kafka main topic. |

### 6.2. Single Log Algorithm

```text
Input: HTTP request with API key and log payload

1. Enforce request size limit.
2. Extract API key from Authorization header.
3. Decode JSON payload.
4. Validate API key and receive TenantContext.
5. Check rate limit using TenantContext/apiKeyId.
6. Validate log fields.
7. Create LogEvent with server-side TenantContext.
8. Publish LogEvent to Kafka.
9. Return 202 Accepted.
```

### 6.3. Batch Log Algorithm

```text
Input: HTTP request with API key and logs[]

1. Enforce request size limit.
2. Extract API key.
3. Decode batch JSON.
4. Validate API key once.
5. Check rate limit for batch count.
6. Validate every item in the batch.
7. If any item invalid, reject the batch before Kafka publish.
8. Build one LogEvent per item.
9. Publish events to Kafka.
10. Return accepted count.
```

Batch ingestion uses all-or-nothing validation before publish. This keeps client-facing error behavior simple. Partial Kafka publish behavior is a reliability decision and should be handled in `05-reliability-design.md`.

### 6.4. Ingestion Error Mapping

| Error | HTTP Response | Enters Kafka? |
| :--- | :--- | :--- |
| Missing API key | `401` | No |
| Invalid API key | `401` | No |
| Rate limit exceeded | `429` | No |
| Invalid JSON request | `400` | No |
| Invalid log field | `400` | No |
| Kafka unavailable | `503` | No or unknown, depending publish result |
| Redis unavailable | Depends policy | Validation can fallback to Control API; rate limit policy decided later. |

## 7. Kafka Event Design

Kafka event is the stable internal handoff between Ingestion API and Log Processor.

```text
LogEvent
├── schemaVersion
├── eventId
├── timestamp
├── receivedAt
├── workspaceId
├── projectId
├── applicationId
├── environment
├── service
├── level
├── message
├── traceId
├── correlationId
└── metadata
```

Design rules:

| Rule | Reason |
| :--- | :--- |
| `schemaVersion` is required. | Allows schema evolution. |
| `eventId` is generated server-side. | Supports idempotency/debug in processor and OpenSearch. |
| Tenant fields are required. | Processor should not call PostgreSQL. |
| `receivedAt` is required. | Supports ingestion/searchable latency measurement. |
| Metadata has guardrails. | Prevents oversized or deeply nested documents. |

## 8. Log Processor Design

Log Processor is responsible for converting Kafka events into OpenSearch documents reliably.

### 8.1. Internal Modules

| Module | Responsibility |
| :--- | :--- |
| Kafka Consumer | Pull raw messages and expose topic/partition/offset context. |
| Event Decoder | Parse JSON and detect malformed message. |
| Event Validator | Validate required internal fields. |
| Batch Buffer | Store valid events until size/interval flush. |
| Bulk Indexer | Send batch to OpenSearch Bulk API. |
| Retry Executor | Retry full bulk failures according to policy. |
| DLQ Publisher | Publish failed raw message/event/item to DLQ topic. |
| Processing Logger | Emit batch-level results and failure context. |

### 8.2. Batch Item Model

Each buffered item must keep both the normalized event and its Kafka context.

| Field | Purpose |
| :--- | :--- |
| `event` | Normalized LogEvent for indexing. |
| `rawPayload` | Original Kafka payload for DLQ/debug. |
| `topic` | Source topic. |
| `partition` | Source partition. |
| `offset` | Source offset. |
| `receivedByProcessorAt` | Processor latency/debug. |

Without Kafka context, partial failure handling cannot DLQ the correct item safely.

### 8.3. Processing Algorithm

```text
For each Kafka message:

1. Decode raw payload.
2. If JSON decode fails:
   - publish DLQ with stage json_decode
   - continue
3. Validate internal LogEvent.
4. If validation fails:
   - publish DLQ with stage validation
   - continue
5. Add BatchItem to buffer.
6. Flush buffer when:
   - batch size reached, or
   - flush interval elapsed
```

Flush algorithm:

```text
Input: BatchItem[]

1. Build OpenSearch bulk request.
2. Execute bulk request.
3. If full request failure:
   - retry according to policy
   - if retry exhausted, DLQ whole batch
4. If partial item failure:
   - DLQ failed items only
   - do not DLQ successful items
5. If success:
   - log indexed count
```

### 8.4. Bulk Index Result Model

| Result Type | Meaning | Required Processor Behavior |
| :--- | :--- | :--- |
| `success` | All items indexed. | Log success and move on. |
| `full_failure` | Bulk request failed without reliable item-level result. | Retry full batch, then DLQ whole batch if exhausted. |
| `partial_failure` | Bulk request returned item-level result with some failed items. | DLQ failed items only. |

This distinction is mandatory. Treating partial failure like full failure would incorrectly DLQ successful items.

## 9. DLQ Design

DLQ is the safety boundary for unprocessable messages. A DLQ event must preserve enough context to debug the failure without re-reading the original Kafka topic.

| Failure Stage | Source | DLQ Payload |
| :--- | :--- | :--- |
| `json_decode` | Raw Kafka message cannot parse. | Raw payload, source topic/partition/offset, reason. |
| `validation` | Parsed event violates internal contract. | Parsed fields if available, raw payload, reason. |
| `indexing` | OpenSearch indexing failed after policy. | Event, tenant fields, item-level reason if available. |

DLQ publisher should not silently drop failed DLQ publish attempts. Exact behavior for DLQ publish failure belongs to Reliability Design.

## 10. Redis Design

Redis is part of core infrastructure, but its design is intentionally narrow.

| Redis Use Case | Key Property | Failure Expectation |
| :--- | :--- | :--- |
| API key validation cache | Short TTL, no raw secret, invalidated on revoke when possible. | Fallback to Control API validation. |
| Rate limit counter | Time-window counter by API key/project. | Fail-open/fail-closed decided by Reliability Design. |
| Short usage counter | Temporary volume tracking. | Can be rebuilt or ignored if not part of billing. |

Redis must never be the only place storing API key state, tenant ownership, log events or search documents.

## 11. Configuration Design

Core configuration should make behavior explicit and testable.

| Config | Owner | Purpose |
| :--- | :--- | :--- |
| `MAX_REQUEST_BYTES` | Ingestion API | Protect request path. |
| `MAX_BATCH_ITEMS` | Ingestion API | Limit client batch ingestion. |
| `API_KEY_CACHE_TTL` | Control/Ingestion | Bound Redis validation staleness. |
| `RATE_LIMIT_WINDOW` | Ingestion API | Rate limit window. |
| `PROCESSOR_BATCH_SIZE` | Log Processor | Flush by size. |
| `PROCESSOR_FLUSH_INTERVAL` | Log Processor | Flush by time. |
| `BULK_RETRY_ATTEMPTS` | Log Processor | Retry full bulk failure. |
| `BULK_RETRY_BACKOFF` | Log Processor | Avoid hammering OpenSearch. |
| `DLQ_TOPIC` | Log Processor | Route failed events. |

## 12. Extension Points

Nice-to-have features are designed as extensions, not as core dependencies.

| Capability | Extension Point | Guardrail |
| :--- | :--- | :--- |
| Retention | Background job or Control API policy config. | Fixed retention values only, no archive/restore. |
| Quota | Redis counters in ingestion path. | Protect ingestion, no billing model. |
| Operational Insights | Control API reads OpenSearch aggregations. | Summary API only. |
| Benchmark | Performance scripts around ingestion/searchable latency. | Evidence for portfolio, not complex load lab. |
| Minimal Web UI | Thin client over Control API. | Demo core flows only. |

Optional alerting, backup/restore and advanced dashboard must not change the core module boundaries.

## 13. Acceptance Criteria For This LLD

| Criterion | Expected Design Result |
| :--- | :--- |
| The design is target-system oriented. | It defines how the system should be built, not what currently exists. |
| Tenant context is server-side. | Client cannot spoof workspace/project/application/environment. |
| Control Plane and Data Plane are separated. | Control API owns metadata; Go services own ingestion/processing. |
| Processor hot path avoids PostgreSQL. | Kafka event already carries tenant context. |
| Redis is useful but bounded. | Cache/rate limit/counter only; no source-of-truth role. |
| Bulk indexing failure semantics are explicit. | Full failure and partial failure produce different behavior. |
| Optional features stay optional. | Alerting/backup/dashboard do not affect core design. |
