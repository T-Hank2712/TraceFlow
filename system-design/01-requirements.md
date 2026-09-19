# Requirements

## 1. Problem & Goals

### 1.1. Problem Statement

Trong hệ thống phân tán, log thường được tạo ra từ nhiều service, container và môi trường khác nhau. Nếu không có một pipeline tập trung để thu thập, xử lý và tìm kiếm log, developer phải kiểm tra từng service riêng lẻ khi điều tra lỗi. Việc này làm quá trình debug chậm, rời rạc và khó truy vết theo ngữ cảnh vận hành.

TraceFlow giải quyết bài toán này bằng cách cung cấp một centralized logging backend. Client application gửi structured logs vào hệ thống bằng API key. Log được xác thực, enrich tenant context phía server, publish vào Kafka, xử lý bất đồng bộ bởi Log Processor, index vào OpenSearch và được tìm kiếm thông qua Control API có kiểm soát quyền truy cập.

### 1.2. Product Goals

TraceFlow không hướng tới việc trở thành một observability suite quá lớn. Mục tiêu của project là xây dựng một backend logging platform đủ sâu để thể hiện năng lực thiết kế hệ thống, bảo mật, xử lý bất đồng bộ, reliability và data lifecycle, nhưng vẫn đủ gọn để hoàn thiện đến nơi đến chốn.

| Goal | Meaning |
| :--- | :--- |
| Centralized log ingestion | Nhiều application có thể gửi structured logs vào một hệ thống tập trung. |
| Asynchronous processing | Kafka tách ingestion khỏi indexing để giảm coupling và hấp thụ traffic spike. |
| Secure API key ingestion | Client application gửi log bằng API key có lifecycle rõ ràng, không dùng JWT của user. |
| Server-side tenant resolution | Workspace, project, application và environment context được xác định phía server, không tin dữ liệu tenant do client gửi lên. |
| Project-scoped log search | User tìm kiếm log thông qua Control API và chỉ thấy dữ liệu trong project được cấp quyền. |
| Reliable processing | Log Processor hỗ trợ validation, batching, OpenSearch Bulk API, retry và Dead Letter Queue. |
| Portfolio-grade evidence | Hệ thống có thể được kiểm thử end-to-end và benchmark throughput/P95 latency khi hoàn thiện. |

### 1.3. In Scope And Out Of Scope

Phạm vi của TraceFlow tập trung vào luồng log end-to-end: từ lúc application gửi log, hệ thống tiếp nhận và xử lý log, đến lúc user tìm kiếm được log theo quyền truy cập. Những phần không trực tiếp làm mạnh hơn luồng này sẽ được giữ ngoài phạm vi ban đầu.

| In Scope | Out Of Scope |
| :--- | :--- |
| User authentication và resource-level access control. | Billing, subscription hoặc payment. |
| Workspace, project và trace application management. | Enterprise organization workflow phức tạp. |
| Secure API key lifecycle cho ingestion path. | OAuth client credentials hoặc public developer portal. |
| Single log và batch log ingestion. | Full SDK ecosystem cho nhiều ngôn ngữ. |
| Kafka-based asynchronous log pipeline. | Custom streaming protocol ngoài HTTP/Kafka. |
| Log Processor với batch processing, bulk indexing, retry và DLQ. | Full replay UI hoặc incident management system. |
| OpenSearch-backed project-scoped log search. | Expose OpenSearch trực tiếp cho end user. |
| Retention, operational insights, alerting và quota ở mức backend có kiểm soát. | Metrics platform hoặc distributed tracing platform đầy đủ. |
| Docker Compose local development stack. | Kubernetes production deployment hoặc multi-region architecture. |
| Minimal Web UI để demo và sử dụng các luồng chính. | Frontend dashboard lớn hoặc design system phức tạp. |

## 2. System Actors & Personas

TraceFlow có ba nhóm actor chính. Mỗi nhóm có nhu cầu khác nhau và đi qua một boundary khác nhau của hệ thống. Thiết kế phải giữ ranh giới này rõ ràng để tránh trộn lẫn user-facing management flow với high-throughput ingestion flow.

| Actor | Persona | Main Need | Entry Point |
| :--- | :--- | :--- | :--- |
| Developer / Engineer | Người điều tra lỗi, debug production issue và tìm log theo ngữ cảnh kỹ thuật. | Search log nhanh theo project, application, environment, service, level, trace ID, correlation ID và time range. | Control API hoặc Web UI. |
| Workspace / Project Admin | Người quản lý tài nguyên, member, quyền truy cập và API key. | Tạo workspace, project, trace application, quản lý member và cấp/revoke API key. | Control API hoặc Web UI. |
| Client Application | Service bên ngoài gửi structured logs vào TraceFlow. | Gửi log ổn định, bảo mật, độ trễ thấp và không cần biết tenant internals. | Ingestion API bằng API key. |

User không truy cập trực tiếp PostgreSQL, Kafka hoặc OpenSearch. Client application không gọi Control API để gửi log và không được tự quyết định workspace/project/application context. Log search luôn đi qua Control API để enforce authentication, authorization và tenant scope.

## 3. Functional Requirements & Business Rules

Functional requirements được mô tả theo capability lớn thay vì liệt kê từng endpoint nhỏ. Mục tiêu của phần này là chốt hệ thống phải làm được gì và các rule quan trọng nào bắt buộc phải giữ đúng. Endpoint chi tiết, schema request/response và event contract sẽ được thiết kế trong tài liệu Contracts.

### 3.1. Identity & Access

TraceFlow cần có authentication cho user-facing APIs để user có thể quản lý tài nguyên và search log. JWT được dùng cho Control API và Web UI, trong khi API key chỉ dành cho ingestion path.

| Requirement | Business Rule |
| :--- | :--- |
| User có thể đăng ký, đăng nhập, refresh session và logout. | JWT chỉ đại diện cho user session, không dùng để client application gửi log. |
| User có thể truy cập workspace/project theo vai trò được cấp. | Mọi thao tác quản lý resource và search log phải kiểm tra quyền tương ứng. |
| Hệ thống hỗ trợ role ở workspace và project. | Project permission quyết định user có được search log trong project đó hay không. |
| Control API là boundary duy nhất cho user-facing management/search. | Không service nào expose business metadata trực tiếp cho user ngoài Control API. |

### 3.2. Resource Management

Resource hierarchy của TraceFlow gồm workspace, project, trace application và API key. Đây là nền tảng để xác định ownership, tenant scope và quyền truy cập.

| Requirement | Business Rule |
| :--- | :--- |
| User có thể tạo và quản lý workspace. | Workspace là tenant boundary cấp cao nhất. |
| User có thể tạo project trong workspace. | Project là scope chính cho membership, log ownership và log search. |
| User có thể tạo trace application trong project. | Trace application đại diện cho workload/application gửi log. |
| User có thể quản lý member theo quyền. | User không có quyền project thì không được search log của project đó. |
| Resource có trạng thái phù hợp như active, archived hoặc deleted. | API key thuộc resource không còn hợp lệ phải bị từ chối trên ingestion path. |

### 3.3. API Key Lifecycle

API key là credential dành riêng cho client application gửi log. Thiết kế API key phải đủ an toàn để không làm lộ secret, nhưng vẫn đủ hiệu quả để Ingestion API validate request trong ingestion path.

| Requirement | Business Rule |
| :--- | :--- |
| Project admin có thể tạo API key cho trace application. | API key chỉ có quyền `ingest:write`, không được dùng cho management/search APIs. |
| Full API key secret chỉ hiển thị một lần khi tạo. | Database không lưu plaintext secret. |
| API key được lưu bằng metadata, prefix và secret hash. | Prefix hỗ trợ lookup/audit; hash dùng để verify secret. |
| API key có thể revoke và expire. | Key revoked, expired hoặc thuộc resource archived/deleted phải bị từ chối. |
| Ingestion API validate API key thông qua Control API. | Control API trả về tenant context hợp lệ khi key còn hiệu lực. |

### 3.4. Log Ingestion

Ingestion API là entry point duy nhất cho client application gửi log. API này cần trả response nhanh sau khi log hợp lệ được đưa vào Kafka, không chờ OpenSearch indexing.

| Requirement | Business Rule |
| :--- | :--- |
| Ingestion API nhận single log request. | Request phải có API key hợp lệ. |
| Ingestion API nhận batch log request. | Batch size và request size phải có giới hạn cấu hình được. |
| Ingestion API validate payload cơ bản. | Log thiếu field bắt buộc hoặc sai format phải bị reject bằng response rõ ràng. |
| Ingestion API enrich log bằng tenant context từ API key. | Không tin workspace, project, application hoặc environment do client gửi lên. |
| Valid enriched event được publish vào Kafka. | Sau khi Kafka accept message, ingestion response không phụ thuộc OpenSearch. |

### 3.5. Log Event Stream

Kafka đóng vai trò là buffer và event stream giữa Ingestion API và Log Processor. Event trong Kafka phải là internal contract đã được enrich tenant context để processor không cần gọi PostgreSQL lấy business metadata trong hot path.

| Requirement | Business Rule |
| :--- | :--- |
| Enriched log event được publish vào main Kafka topic. | Event phải có `eventId`, `timestamp`, `receivedAt`, tenant context và log content. |
| Kafka topic chính và DLQ topic phải tách riêng. | Bad message không được làm nghẽn main processing flow. |
| Event schema phải có version. | Schema evolution cần có đường nâng cấp rõ ràng trong tương lai. |
| Kafka partitioning phải hỗ trợ scaling. | Partition key sẽ được chốt trong Reliability Design để cân bằng ordering và throughput. |

### 3.6. Log Processing & Indexing

Log Processor chịu trách nhiệm biến Kafka event thành searchable document trong OpenSearch. Processor cần xử lý theo batch để tăng throughput và giảm số request indexing.

| Requirement | Business Rule |
| :--- | :--- |
| Processor consume Kafka event và validate internal event. | Invalid JSON hoặc invalid internal event phải đi DLQ. |
| Processor buffer valid events thành batch. | Batch phải giữ được original Kafka message context để xử lý lỗi và DLQ đúng item. |
| Batch được flush theo size hoặc interval. | Cả batch size và flush interval phải cấu hình được. |
| Processor index batch bằng OpenSearch Bulk API. | Không index từng event riêng lẻ trong steady-state processing. |
| Full bulk failure được retry theo policy. | Nếu vẫn thất bại sau retry, toàn bộ batch được đưa vào DLQ. |
| Partial bulk failure được xử lý theo item. | Chỉ item lỗi đi DLQ; item đã index thành công không được đưa vào DLQ. |

### 3.7. Dead Letter Queue

DLQ là safety net cho những event không thể xử lý thành công. Mục tiêu của DLQ không chỉ là tránh nghẽn pipeline, mà còn giữ đủ bằng chứng để debug hoặc replay thủ công trong tương lai.

| Failure Case | Expected Behavior |
| :--- | :--- |
| Kafka message không parse được JSON. | Gửi original payload vào DLQ với failure stage `json_decode`. |
| Internal log event thiếu field bắt buộc hoặc sai format. | Gửi event vào DLQ với failure stage `validation`. |
| OpenSearch full bulk failure sau retry. | Gửi toàn batch vào DLQ với failure stage `indexing`. |
| OpenSearch partial bulk failure. | Chỉ gửi item lỗi vào DLQ, kèm item-level failure reason. |
| DLQ publish thất bại. | Hành vi retry/dừng/continue sẽ được chốt trong Reliability Design. |

### 3.8. Log Search

User search log thông qua Control API. Control API chịu trách nhiệm xác thực user, kiểm tra quyền project, build query đã scope theo tenant context và gọi OpenSearch thay cho user.

| Requirement | Business Rule |
| :--- | :--- |
| User có thể search log theo project. | Query luôn scope theo workspace/project. |
| Search hỗ trợ filter theo application, environment, level, service, trace ID, correlation ID và time range. | Filter không được làm mất tenant scope bắt buộc. |
| Search API hỗ trợ pagination. | Không trả kết quả không giới hạn trong một request. |
| Search API có default time range hợp lý. | Tránh query quá rộng khi user không truyền `from`/`to`. |
| OpenSearch không expose trực tiếp cho user. | Mọi search đều đi qua Control API để enforce RBAC. |

### 3.9. Operations & Supporting Capabilities

Ngoài pipeline chính, TraceFlow cần một số capability hỗ trợ để project có thể vận hành, kiểm thử và demo end-to-end trong local development.

| Requirement | Business Rule |
| :--- | :--- |
| Docker Compose chạy được local stack. | PostgreSQL, Kafka, OpenSearch và các service chính phải có config rõ ràng. |
| Service có health check cơ bản. | Developer cần biết dependency nào sẵn sàng hoặc đang lỗi. |
| Hệ thống có retention policy ở mức có kiểm soát. | Log hết hạn có thể bị xoá khỏi OpenSearch; archive storage không nằm trong scope ban đầu. |
| Hệ thống có operational insights backend ở mức cơ bản. | Insight lấy từ log data, không biến thành dashboard/metrics platform lớn. |
| Hệ thống có alert rule backend đơn giản. | Alerting không bao gồm notification system production-grade trong scope ban đầu. |
| Hệ thống có quota/rate limit ở mức bảo vệ ingestion path. | Quota phục vụ bảo vệ hệ thống, không phải billing. |

## 4. Non-Functional Requirements

Non-functional requirements mô tả chất lượng hệ thống cần đạt được. Đây là phần định hướng cho các quyết định ở High-Level Design, Low-Level Design, Reliability Design và Testing Strategy.

| Category | Requirement | Design Implication |
| :--- | :--- | :--- |
| Scale | Ingestion API cần xử lý nhiều request đồng thời và không giữ state nghiệp vụ dài hạn. | Ingestion API nên stateless để scale ngang. |
| Throughput | Processor cần xử lý log theo batch thay vì per-message indexing. | Dùng batch buffer và OpenSearch Bulk API. |
| Latency | Ingestion response không được phụ thuộc vào OpenSearch indexing. | Kafka nằm giữa ingestion và processor; search là eventually consistent. |
| Reliability | Log đã được accepted vào Kafka không nên mất silently. | Processor cần retry, DLQ và log batch-level result rõ ràng. |
| Failure Isolation | Bad message không được làm nghẽn toàn bộ consumer loop. | Invalid JSON, invalid event và indexing failure đi DLQ theo policy. |
| Security | User-facing API dùng JWT; ingestion dùng API key. | Tách rõ user session và application credential. |
| API Key Safety | API key secret không được lưu plaintext và chỉ hiển thị một lần. | Lưu prefix + hash; full secret không thể lấy lại sau khi tạo. |
| Tenant Isolation | User chỉ thấy log trong project được cấp quyền. | Control API enforce RBAC và luôn inject workspace/project scope vào query. |
| Consistency | Log ingestion và log search không nhất thiết đồng bộ ngay lập tức. | Hệ thống chấp nhận eventual consistency giữa Kafka và OpenSearch. |
| Availability | Trong phạm vi project, ưu tiên ingestion path tiếp tục nhận log nếu Kafka còn hoạt động dù OpenSearch tạm thời lỗi. | OpenSearch failure được xử lý ở processor bằng retry/DLQ. |
| CAP Thinking | Khi downstream search storage lỗi, hệ thống ưu tiên availability của ingestion hơn immediate consistency của search. | Log có thể chưa search được ngay, nhưng không nên làm ingestion fail nếu Kafka vẫn nhận message. |
| Maintainability | Boundary giữa Control Plane và Data Plane phải rõ ràng. | Control API quản lý business metadata; Go services xử lý ingestion/processing. |
| Observability | Hệ thống cần đủ log vận hành để debug pipeline. | Processor logs cần có batch size, indexed count, failed count, retry attempts và DLQ result. |

### 4.1. Initial Reliability Targets

TraceFlow không đặt mục tiêu SLA production-grade trong phạm vi ban đầu, nhưng vẫn cần target rõ để thiết kế không mơ hồ.

| Area | Target |
| :--- | :--- |
| Ingestion availability | Local/dev stack phải xử lý được request hợp lệ khi Control API, Kafka và Ingestion API sẵn sàng. |
| Accepted log durability | Khi Kafka publish thành công, event phải được xử lý thành công hoặc có record trong DLQ nếu thất bại. |
| Search consistency | Log hợp lệ có thể search được sau khi processor index thành công; không yêu cầu read-after-write immediate consistency. |
| Failure visibility | Các failure path chính phải có log và DLQ evidence để kiểm tra. |

## 5. Back-of-the-Envelope Estimation

Các ước lượng dưới đây không phải cam kết hiệu năng cuối cùng. Chúng là baseline để định hướng batch size, retention, OpenSearch storage, Kafka throughput và benchmark sau này.

### 5.1. Traffic Assumptions

TraceFlow nên được thiết kế để chứng minh được lợi ích của Kafka và batch processing trong phạm vi local/dev hoặc portfolio benchmark. Mục tiêu không phải traffic enterprise cực lớn, mà là một mức tải đủ thực tế để làm rõ các trade-off thiết kế.

| Metric | Initial Target | Reason |
| :--- | :--- | :--- |
| Average ingestion rate | 100-300 logs/s | Phù hợp với local/dev benchmark và scope portfolio. |
| Peak ingestion rate | 1,000 logs/s | Đủ để chứng minh Kafka buffering và processor batching có ý nghĩa. |
| Average log size | 1-2 KB | Structured log có tenant fields, message, correlation fields và metadata vừa phải. |
| Batch size | 100-500 events | Cân bằng throughput, memory usage và searchable latency. |
| Flush interval | 1-5 seconds | Giới hạn thời gian log nằm trong buffer trước khi index. |

### 5.2. Storage Estimation

Dung lượng log phụ thuộc chủ yếu vào ingestion rate, kích thước trung bình mỗi log và retention. Công thức ước lượng raw data:

```text
daily_storage = logs_per_second * average_log_size * 86,400
```

| Scenario | Raw Estimate |
| :--- | :--- |
| 100 logs/s * 1 KB/log | Khoảng 8.6 GB/day |
| 300 logs/s * 1 KB/log | Khoảng 25.9 GB/day |
| 1,000 logs/s * 1 KB/log | Khoảng 86.4 GB/day |

OpenSearch thường cần nhiều hơn raw payload vì index metadata, inverted index, replicas và field mapping overhead. Vì vậy retention cần được giới hạn theo project hoặc application, thay vì giữ log vô thời hạn.

### 5.3. Bandwidth, Memory And Worker Sizing

Các con số dưới đây giúp định hướng cấu hình ban đầu. Chúng sẽ được kiểm chứng lại trong Testing Strategy và benchmark thực tế.

| Area | Initial Assumption |
| :--- | :--- |
| Kafka bandwidth | 1,000 logs/s với 1 KB/log tương đương khoảng 1 MB/s raw payload, chưa tính protocol overhead. |
| Ingestion API memory | Nên tránh giữ batch lớn trong request path; batch ingestion request vẫn phải có size limit. |
| Processor memory | Batch buffer phụ thuộc batch size, average log size và số partition đang xử lý đồng thời. |
| OpenSearch pressure | Bulk size cần cấu hình được để tránh tạo request quá lớn hoặc quá nhiều request nhỏ. |
| PostgreSQL load | PostgreSQL chủ yếu phục vụ auth, resource metadata và API key validation, không nằm trên log write hot path. |
| Redis usage | Redis có thể giảm tải validation/cache/rate limit, nhưng không phải dependency bắt buộc cho core pipeline ban đầu. |

## 6. Assumptions & Constraints

Phần này chốt các giả định và ràng buộc nền tảng để các tài liệu thiết kế sau không đi lệch hướng. Nếu một giả định thay đổi, HLD, Contracts, LLD và Roadmap cần được cập nhật tương ứng.

| Type | Decision |
| :--- | :--- |
| Control Plane | ASP.NET Core chịu trách nhiệm authentication, resource management, RBAC, API key lifecycle và log search. |
| Data Plane | Go chịu trách nhiệm ingestion gateway, Kafka producer/consumer và log processing. |
| Business Storage | PostgreSQL là source of truth cho user, workspace, project, application, membership và API key metadata. |
| Event Streaming | Kafka là buffer chính giữa ingestion và processing. |
| Search Storage | OpenSearch là backend chính cho log indexing, filtering, time range query và search. |
| Supporting Infrastructure | Redis có thể dùng cho API key validation cache, rate limiting hoặc counter ngắn hạn, nhưng không phải source of truth. |
| Local Development | Docker Compose là môi trường local development và demo chính. |
| Consistency Model | Log search là eventually consistent vì ingestion và indexing được xử lý bất đồng bộ. |
| Security Boundary | User dùng JWT với Control API; client application dùng API key với Ingestion API. |
| Tenant Boundary | Workspace, project, application và environment phải đi cùng log event từ ingestion đến search. |
| Scope Control | Không làm billing, Kubernetes production deployment, archive storage, full SDK ecosystem hoặc frontend dashboard lớn trong scope ban đầu. |

### 6.1. Open Decisions

Một số quyết định không nên chốt quá sớm trong Requirements vì cần thêm thiết kế hoặc benchmark thực tế. Các quyết định này sẽ được xử lý trong những tài liệu tiếp theo.

| Decision | Owner Document | Note |
| :--- | :--- | :--- |
| Kafka partition key | Reliability Design | Cần cân bằng ordering, throughput và khả năng scale consumer group. |
| DLQ publish failure behavior | Reliability Design | Cần chốt processor nên retry, pause, stop hay continue trong từng tình huống. |
| Exact benchmark target | Testing Strategy | Cần dựa trên local stack hoặc môi trường benchmark ổn định. |
| Redis adoption phase | Implementation Roadmap | Redis hữu ích cho cache/rate limiting, nhưng không bắt buộc cho core pipeline đầu tiên. |
| Retention execution strategy | Reliability / Operations Design | Requirements chỉ chốt retention nằm trong scope có kiểm soát; cách chạy job/xóa dữ liệu sẽ thiết kế sau. |
