# Security And Tenancy Design

## 1. Mục Tiêu Security

Security And Tenancy Design mô tả cách TraceFlow bảo vệ user, API key, tenant boundary và log data trong core scope:

```text
Multi-tenant log ingestion and search pipeline
```

Mục tiêu chính là đảm bảo mỗi request được xác thực bằng đúng loại credential, mỗi thao tác được kiểm tra quyền ở đúng resource boundary, và mọi log event đều mang tenant context do server xác định. Client application không được tự quyết định `workspaceId`, `projectId`, `applicationId` hoặc `environment`.

Security của TraceFlow dựa trên bốn nguyên tắc:

| Nguyên tắc | Ý nghĩa |
| :--- | :--- |
| Credential separation | User dùng JWT; client application dùng API key. Hai loại credential không thay thế nhau. |
| Server-side tenant resolution | Tenant context được resolve từ API key hợp lệ, không lấy từ client payload. |
| Project-scoped authorization | User search log phải có quyền với project tương ứng. |
| No direct storage exposure | User/client không truy cập trực tiếp PostgreSQL, Redis, Kafka hoặc OpenSearch. |

TraceFlow không hướng tới enterprise security platform, nhưng core path phải đủ chặt để tránh spoof tenant, lộ API key secret, bypass RBAC hoặc search nhầm dữ liệu giữa project.

## 2. Trust Boundaries

Trust boundary giúp xác định thành phần nào được tin, thành phần nào không được tin và dữ liệu nào cần validate lại.

| Boundary | Caller | Callee | Trust Level | Security Requirement |
| :--- | :--- | :--- | :--- | :--- |
| User-facing Control API | User/Web UI/API client | Control API | Không tin request cho đến khi JWT hợp lệ. | JWT authentication và RBAC. |
| Public Ingestion API | Client Application | Ingestion API | Không tin payload và tenant fields. | API key authentication, payload validation, rate limit. |
| Internal validation | Ingestion API | Control API | Chỉ tin khi có internal service credential. | Service-to-service authentication. |
| Event stream | Ingestion API | Kafka/Processor | Tin schema sau khi được enrich, vẫn validate lại ở processor. | Internal event validation. |
| Search storage | Control API | OpenSearch | Chỉ Control API được query thay user. | Tenant filter bắt buộc và project access check. |
| Cache/rate limit | Control/Ingestion API | Redis | Không phải source of truth. | TTL, no raw secret, fallback policy. |

Mọi dữ liệu đi qua public boundary phải được validate. Internal boundary vẫn cần authentication vì lỗi cấu hình hoặc service giả mạo có thể phá tenant isolation.

## 3. Identity Model

TraceFlow có hai loại identity chính: user identity và application identity. Hai loại này có mục đích khác nhau và không được dùng lẫn nhau.

| Identity | Credential | Đại diện cho | Dùng để |
| :--- | :--- | :--- | :--- |
| User identity | JWT access token | Người dùng đăng nhập. | Quản lý resource, member, API key và search log. |
| Application identity | API key | Trace application gửi log. | Gửi single log hoặc batch logs vào Ingestion API. |
| Service identity | Internal secret hoặc cơ chế tương đương. | Service nội bộ trong TraceFlow. | Ingestion API gọi internal validation endpoint của Control API. |

JWT không được dùng để gửi log từ client application. API key không được dùng để gọi API quản trị hoặc search. Service credential không được expose ra bên ngoài.

## 4. User Authentication Với JWT

JWT được dùng cho Control API và Minimal Web UI. Token đại diện cho user session và chỉ có ý nghĩa trong user-facing control plane.

Flow tổng quát:

```text
User
-> Login
-> Control API verifies credentials
-> Issue access token + refresh token
-> User calls Control API with Bearer token
```

Control API phải validate JWT trước khi xử lý request quản trị hoặc search. Sau khi xác thực, API vẫn phải kiểm tra authorization theo workspace/project. Authentication chỉ trả lời user là ai; authorization mới trả lời user được làm gì.

| Requirement | Design Rule |
| :--- | :--- |
| JWT dùng cho user-facing APIs. | Không dùng JWT cho ingestion path. |
| Access token nên có thời gian sống ngắn. | Giảm rủi ro khi token bị lộ. |
| Refresh token/session phải có khả năng revoke. | Logout hoặc đổi mật khẩu có thể chấm dứt session. |
| JWT claims không thay thế database authorization. | Role/resource access vẫn cần kiểm tra từ Control API metadata. |

## 5. API Key Security Model

API key là credential dành riêng cho client application gửi log. Đây là boundary quan trọng nhất của ingestion path vì API key quyết định tenant context của log event.

### 5.1. API Key Creation

Khi tạo API key, Control API sinh full secret và chỉ trả về một lần.

```text
Project Admin
-> Create API key
-> Control API generates prefix + secret
-> Hash secret with server-side secret/pepper
-> Store metadata + prefix + hash
-> Return full secret once
```

Sau thời điểm tạo, hệ thống không thể trả lại full secret. User chỉ xem được metadata như name, prefix, status, environment, expiration và last used time.

### 5.2. API Key Storage

| Data | Storage Rule |
| :--- | :--- |
| Full secret | Chỉ hiển thị một lần khi tạo, không lưu plaintext. |
| Prefix | Được lưu để lookup và audit. |
| Secret hash | Được lưu trong PostgreSQL để verify. |
| Environment | Lưu trong API key metadata để gắn tenant context. |
| Status | Active, revoked hoặc expired. |

API key hash phải dùng server-side secret/pepper hoặc cơ chế tương đương để giảm rủi ro nếu database bị lộ. Log nội bộ không được ghi full secret.

### 5.3. API Key Validation

Validation API key phải kiểm tra đủ cả credential và resource chain.

```text
1. Parse API key prefix.
2. Lookup API key metadata by prefix.
3. Verify secret hash.
4. Check key status: not revoked, not expired.
5. Check TraceApplication status.
6. Check Project status.
7. Check Workspace status.
8. Return TenantContext.
```

Nếu bất kỳ bước nào fail, request ingestion bị từ chối. Validation response không được trả secret, hash hoặc metadata không cần thiết.

### 5.4. API Key Revocation And Expiration

Revocation phải có hiệu lực trên ingestion path. Nếu Redis đang cache validation result, Control API cần invalidate cache khi có thể hoặc dùng TTL đủ ngắn để stale cache không kéo dài.

| Case | Expected Behavior |
| :--- | :--- |
| API key revoked | Ingestion request bị từ chối. |
| API key expired | Ingestion request bị từ chối. |
| Application archived/deleted | API key bị xem là invalid. |
| Project/workspace archived/deleted | API key bị xem là invalid. |
| Redis cache còn entry cũ | TTL/invalidation giới hạn thời gian stale. |

## 6. Service-To-Service Authentication

Ingestion API gọi Control API để validate API key. Đây là internal boundary và cần authentication riêng để tránh endpoint validation bị gọi trái phép.

Thiết kế core dùng internal service secret hoặc cơ chế tương đương:

```http
POST /internal/api-keys/validate
X-Internal-Secret: {internalSecret}
```

Control API phải reject request internal nếu thiếu hoặc sai internal credential. Internal endpoint không được public trong tài liệu user-facing API và không nên expose qua gateway public nếu có.

| Requirement | Design Rule |
| :--- | :--- |
| Internal endpoint chỉ dành cho service nội bộ. | Bảo vệ bằng `X-Internal-Secret` hoặc mTLS trong tương lai. |
| Internal secret không hardcode trong code. | Lấy từ environment/config secret. |
| Internal response chỉ trả tenant context tối thiểu. | Không trả secret/hash/database details. |
| Internal failure không leak implementation detail. | Trả lỗi an toàn và log chi tiết ở server. |

## 7. Tenant Resolution Và Isolation

Tenant isolation là yêu cầu security trung tâm của TraceFlow. Mọi log event phải có tenant context rõ ràng từ lúc vào Kafka đến lúc được search.

### 7.1. Tenant Context Source

Tenant context hợp lệ chỉ có một nguồn:

```text
API key validation result from Control API
```

Client request có thể gửi các field như `service`, `level`, `message`, `traceId`, `correlationId`, `metadata`, nhưng không được quyết định tenant context.

| Field | Source Of Truth |
| :--- | :--- |
| `workspaceId` | Control API validation result. |
| `projectId` | Control API validation result. |
| `applicationId` | Control API validation result. |
| `environment` | API key/application metadata trong Control API. |
| `service` | Client payload, sau validation. |
| `level` | Client payload, sau validation. |
| `message` | Client payload, sau validation. |

Nếu client gửi `workspaceId`, `projectId`, `applicationId` hoặc `environment` trong ingestion payload, Ingestion API không được dùng các field đó làm source of truth. Có thể ignore hoặc reject theo validation policy, nhưng không được tin.

### 7.2. Tenant Context Propagation

Tenant context phải đi cùng log event trong toàn bộ pipeline.

```text
API key validation
-> TenantContext
-> Enriched Kafka LogEvent
-> OpenSearch LogDocument
-> Project-scoped SearchResult
```

Log Processor không gọi PostgreSQL để resolve tenant. Nếu Kafka event thiếu tenant field bắt buộc, processor coi event là invalid và đưa vào DLQ với stage `validation`.

## 8. RBAC Model

RBAC của TraceFlow cần đủ rõ để bảo vệ workspace/project mà không biến project thành enterprise IAM phức tạp.

### 8.1. Resource Levels

| Level | Role Purpose |
| :--- | :--- |
| Workspace | Quản lý tenant cấp cao, workspace member và project creation. |
| Project | Quản lý project, application, API key và quyền search log. |
| Application | Resource con dùng cho ingestion context, không nhất thiết có membership riêng trong core. |

### 8.2. Permission Groups

| Permission Group | Examples |
| :--- | :--- |
| Workspace management | Update workspace, invite/remove member, create project. |
| Project management | Update project, manage project member, create application. |
| API key management | Create/list/revoke API key. |
| Log search | Search logs within project. |

Thiết kế core có thể map các permission group này vào role cụ thể như Owner, Admin, Manager, Viewer tùy implementation. Điểm quan trọng là handler không tự suy luận role rời rạc; mọi check phải đi qua access service.

## 9. Search Authorization

Search là nơi dễ xảy ra data leak giữa tenant nếu query không được scope chặt. Control API phải enforce access trước khi query OpenSearch.

Search flow bắt buộc:

```text
1. Validate JWT.
2. Load user context.
3. Check project access.
4. Build OpenSearch query with mandatory filters:
   - workspaceId
   - projectId
5. Add optional filters.
6. Execute query.
7. Return scoped results.
```

User-provided filters không được override tenant filters. Nếu user truyền `workspaceId` hoặc `projectId` không khớp route/resource context, request phải bị reject hoặc ignored theo policy an toàn.

| Risk | Mitigation |
| :--- | :--- |
| User search project không có quyền. | Project access check trước OpenSearch query. |
| Query thiếu tenant filter. | Query builder bắt buộc inject `workspaceId` và `projectId`. |
| User truyền filter để vượt scope. | Route/context là source of truth, không tin query tenant fields. |
| OpenSearch expose trực tiếp. | Chỉ Control API được query thay user. |

## 10. Redis Security Considerations

Redis nằm trong core scope nhưng chỉ là supporting infrastructure. Redis không được lưu secret nhạy cảm hoặc trở thành nguồn quyền truy cập cuối cùng.

| Redis Data | Security Rule |
| :--- | :--- |
| API key validation cache | Không lưu full secret hoặc secret hash. |
| Cache key | Không chứa raw API key secret. |
| Tenant context cache | TTL ngắn, invalidate khi revoke nếu có thể. |
| Rate limit counter | Không chứa payload log nhạy cảm. |
| Usage counter | Chỉ lưu số đếm ngắn hạn. |

Redis compromise không nên cho attacker lấy được full API key secret. Nếu Redis bị mất dữ liệu, hệ thống phải có thể rebuild cache từ Control API/PostgreSQL.

## 11. Data Protection And Secret Handling

TraceFlow xử lý nhiều loại dữ liệu nhạy cảm: password hash, refresh token, JWT, API key secret, internal service secret và log metadata. Không phải dữ liệu nào cũng cần cùng mức bảo vệ, nhưng secret phải có rule rõ ràng.

| Data | Protection Rule |
| :--- | :--- |
| Password | Lưu hash bằng thuật toán phù hợp, không lưu plaintext. |
| Refresh token | Lưu an toàn, có thể revoke. |
| JWT access token | Thời gian sống ngắn, không log token. |
| API key full secret | Chỉ hiển thị một lần, không lưu plaintext, không log. |
| API key hash | Lưu trong PostgreSQL, không trả qua API. |
| Internal service secret | Lấy từ environment/secret config, không hardcode. |
| Log metadata | Validate size/depth, tránh ghi secret không cần thiết trong server logs. |

Server logs phải tránh ghi credential. Khi cần debug, chỉ dùng `apiKeyPrefix`, `apiKeyId`, `requestId`, `eventId` hoặc tenant IDs.

## 12. Security Failure Handling

Security failure phải trả response an toàn cho caller và log đủ thông tin cho server-side investigation.

| Failure | Client Response | Server Log |
| :--- | :--- | :--- |
| Missing JWT | `401 Unauthorized` | RequestId, route. |
| Invalid JWT | `401 Unauthorized` | RequestId, reason generic. |
| Authenticated but forbidden | `403 Forbidden` hoặc `404 Not Found` theo policy. | UserId, resourceId, action. |
| Missing API key | `401 Unauthorized` | RequestId, route. |
| Invalid API key | `401 Unauthorized` | API key prefix nếu parse được, reason generic. |
| Revoked/expired API key | `401 Unauthorized` | API key id/prefix, status. |
| Internal secret invalid | `401 Unauthorized` hoặc `403 Forbidden`. | Caller metadata nếu có. |
| Tenant mismatch attempt | `400` hoặc `403`. | User/client context, attempted fields. |

Client response không nên tiết lộ key tồn tại hay không, hash mismatch hay resource archived. Server log có thể chi tiết hơn nhưng không được chứa secret.

## 13. Security Acceptance Criteria

Security design được xem là đạt khi các behavior sau có thể được chứng minh bằng test hoặc manual verification.

| Scenario | Expected Result |
| :--- | :--- |
| User không có JWT gọi Control API. | Request bị từ chối. |
| User có JWT nhưng không có quyền project. | Không search được log của project. |
| Client dùng API key revoked/expired. | Ingestion request bị từ chối. |
| Client gửi `workspaceId/projectId/applicationId` trong payload. | Hệ thống không dùng các field đó làm tenant source of truth. |
| API key secret sau khi tạo. | Không thể lấy lại full secret qua list/detail endpoint. |
| API key bị revoke khi Redis đang cache. | Cache bị invalidate hoặc hết hiệu lực trong TTL ngắn. |
| Log Processor nhận event thiếu tenant fields. | Event bị đưa vào DLQ stage `validation`. |
| Search query không có tenant filter. | Query builder không cho phép hoặc tự inject filter bắt buộc. |
| User/client cố truy cập OpenSearch trực tiếp. | Không có public access path. |
| Server logs khi auth fail. | Không chứa full API key, JWT hoặc internal secret. |

Tài liệu này là nền cho `07-testing-strategy.md` xây dựng security tests cho JWT auth, RBAC, API key lifecycle, tenant spoofing, Redis cache staleness và project-scoped search.
