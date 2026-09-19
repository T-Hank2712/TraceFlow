# High-Level Design

## 1. Mục Tiêu Thiết Kế

High-Level Design mô tả kiến trúc tổng thể của TraceFlow ở mức component, service boundary, luồng dữ liệu chính và các quyết định kiến trúc nền tảng.

Tài liệu này không đi vào chi tiết class, function hoặc implementation cụ thể. Các chi tiết đó sẽ được mô tả trong Low-Level Design.

Mục tiêu của HLD là làm rõ:

- TraceFlow gồm những thành phần chính nào.
- Mỗi thành phần chịu trách nhiệm gì.
- Dữ liệu đi qua hệ thống theo luồng tổng thể ra sao.
- Ranh giới giữa Control Plane và Data Plane nằm ở đâu.
- Storage nào được dùng cho loại dữ liệu nào.
- Hệ thống đảm bảo security, tenant isolation, reliability và scalability ở mức kiến trúc như thế nào.

## 2. Design Assumptions

Các giả định thiết kế dưới đây giúp định hướng kiến trúc tổng thể của TraceFlow trước khi đi vào contract, LLD và reliability design.

| Câu hỏi thiết kế | Câu trả lời cho TraceFlow |
| :--- | :--- |
| How many users? Growing how fast? | TraceFlow ban đầu hướng tới portfolio/local-demo scale: số user quản trị không lớn, nhưng log ingestion có thể tăng nhanh theo số application và traffic của từng application. Thiết kế ưu tiên khả năng scale ingestion và processing hơn là scale số lượng user quản trị. |
| Read-heavy or write-heavy? | Hệ thống write-heavy ở ingestion path vì application liên tục gửi log. Search path có tần suất thấp hơn nhưng cần filter/query tốt khi developer điều tra lỗi. |
| What can never lose? | Business metadata trong PostgreSQL không được mất. Log event đã được Ingestion API accepted không nên mất; nếu không index được thì phải được giữ trong Kafka hoặc DLQ. API key secret không được lưu plaintext và full secret chỉ hiển thị một lần. |
| How much latency can you afford? | Ingestion response cần thấp vì client application không nên chờ OpenSearch indexing. End-to-end searchable latency có thể cao hơn do pipeline bất đồng bộ và batch processing. |
| What does it cost? | Kafka, OpenSearch và bulk indexing tăng độ phức tạp nhưng đổi lại có async processing, throughput và search tốt hơn. Redis được xem là optimization cho cache/rate limiting, không phải dependency bắt buộc của core pipeline. |

## 3. System Context

TraceFlow là một centralized logging backend platform cho distributed applications.

Hệ thống có ba nhóm tác nhân chính:

| Tác nhân | Vai trò | Cách tương tác với hệ thống |
| :--- | :--- | :--- |
| User / Developer | Tìm kiếm log, điều tra lỗi, quản lý workspace/project/application. | Gọi Control API thông qua Web UI hoặc API client. |
| Workspace / Project Admin | Quản lý member, quyền truy cập, trace application và API key. | Gọi Control API bằng JWT access token. |
| Client Application | Gửi structured logs vào TraceFlow. | Gọi Ingestion API bằng API key. |

TraceFlow không cho user hoặc client application truy cập trực tiếp PostgreSQL, Kafka hoặc OpenSearch. Mọi thao tác quản trị và search log của user đều đi qua Control API. Mọi log submission từ application bên ngoài đều đi qua Ingestion API.

## 4. Kiến Trúc Tổng Thể

TraceFlow được chia thành hai mặt phẳng trách nhiệm:

| Mặt phẳng | Thành phần chính | Trách nhiệm |
| :--- | :--- | :--- |
| Control Plane | Control API | Authentication, authorization, resource management, API key lifecycle, project-scoped log search. |
| Data Plane | Ingestion API, Kafka, Log Processor | Log ingestion, API key validation call, event streaming, batch processing, OpenSearch indexing, DLQ. |

Sơ đồ tổng thể:

```text
                         +----------------+
                         |   User / UI    |
                         +--------+-------+
                                  |
                                  | JWT
                                  v
                         +----------------+
                         |  Control API   |
                         +---+--------+---+
                             |        |
                             |        |
                             v        v
                     +----------+  +-------------+
                     |PostgreSQL|  | OpenSearch  |
                     +----------+  +-------------+


+--------------------+
| Client Application |
+---------+----------+
          |
          | Authorization: ApiKey {secret}
          v
+--------------------+        internal validation        +----------------+
|   Ingestion API    +----------------------------------->+  Control API   |
+---------+----------+                                    +----------------+
          |
          | enriched log event
          v
+--------------------+
|       Kafka        |
+---------+----------+
          |
          | consume
          v
+--------------------+        bulk index        +-------------+
|   Log Processor    +------------------------->+ OpenSearch  |
+---------+----------+                          +-------------+
          |
          | failed event
          v
+--------------------+
|     DLQ Topic      |
+--------------------+
```

## 5. Component Responsibilities

### 5.1. Control API

Control API là control plane của TraceFlow.

Trách nhiệm chính:

- User authentication và session management.
- Workspace management.
- Project management.
- Trace application management.
- Workspace/project membership và RBAC.
- API key lifecycle management.
- Internal API key validation cho Ingestion API.
- Project-scoped log search.
- Enforce access control trước khi query OpenSearch.
- Là lớp user-facing duy nhất cho management và search API.

Control API sử dụng PostgreSQL làm source of truth cho business data và control metadata. Control API có thể query OpenSearch để phục vụ search log, nhưng user không được truy cập OpenSearch trực tiếp.

### 5.2. Ingestion API

Ingestion API là entry point cho Client Application gửi log.

Trách nhiệm chính:

- Nhận single log và batch logs.
- Đọc API key từ `Authorization: ApiKey {secret}`.
- Reject request thiếu API key hoặc sai format.
- Gọi Control API internal endpoint để validate API key.
- Nhận tenant context hợp lệ từ Control API.
- Không tin tenant context do client gửi lên.
- Validate request body, required fields, batch size và request body size.
- Normalize các field cơ bản.
- Enrich log event bằng workspace, project, application và environment context.
- Publish enriched log event vào Kafka.
- Trả response sau khi Kafka publish thành công.

Ingestion API không quản lý user, workspace, project hoặc application. Ingestion API cũng không index log trực tiếp vào OpenSearch.

### 5.3. Kafka

Kafka là event streaming layer và buffer giữa ingestion và processing.

Trách nhiệm chính:

- Nhận enriched log event từ Ingestion API.
- Tách ingestion path khỏi indexing path.
- Cho phép Log Processor xử lý log bất đồng bộ.
- Hỗ trợ tăng throughput thông qua partitioning.
- Hỗ trợ scale Log Processor bằng consumer group.
- Lưu topic riêng cho main log events và DLQ.

Kafka không phải nơi lưu trữ log dài hạn. Log sau khi xử lý được index vào OpenSearch.

### 5.4. Log Processor

Log Processor là worker xử lý log event từ Kafka.

Trách nhiệm chính:

- Consume enriched log event từ Kafka main topic.
- Parse và validate internal log event.
- Normalize log event trước khi index.
- Buffer valid events thành batch.
- Flush batch theo batch size hoặc flush interval.
- Index batch vào OpenSearch bằng Bulk API.
- Retry full bulk indexing failure theo policy.
- Đưa toàn batch vào DLQ nếu full bulk failure vẫn thất bại sau retry.
- Phát hiện partial bulk failure.
- Chỉ đưa item failed vào DLQ khi partial failure xảy ra.
- Không đưa item đã index thành công vào DLQ.
- Ghi log rõ ràng về batch size, indexed count, failed count, retry attempt và DLQ result.

Log Processor không gọi PostgreSQL để lấy business metadata. Tenant context phải đã được enrich từ Ingestion API trước khi event vào Kafka.

### 5.5. PostgreSQL

PostgreSQL là source of truth cho business data và control metadata.

Dữ liệu chính:

- User.
- Refresh token/session.
- Workspace.
- Workspace member.
- Workspace invitation.
- Project.
- Project member.
- Project invitation.
- Trace application.
- API key metadata và secret hash.
- Retention configuration nếu thuộc scope hiện tại.

PostgreSQL không lưu log event high-throughput.

### 5.6. OpenSearch

OpenSearch là search storage cho log document.

Trách nhiệm chính:

- Lưu enriched log document.
- Hỗ trợ project-scoped log search.
- Hỗ trợ filter theo application, environment, level, service, trace ID, correlation ID và time range.
- Hỗ trợ sort và pagination theo timestamp.
- Là backend cho search và analytics cơ bản.

OpenSearch không được expose trực tiếp cho user hoặc client application. Mọi query từ user phải đi qua Control API để enforce authentication, authorization và tenant scope.

### 5.7. Redis

Redis là supporting infrastructure có thể được sử dụng trong các giai đoạn sau.

Vai trò phù hợp:

- Cache kết quả API key validation.
- Rate limiting theo API key, application hoặc project.
- Counter ngắn hạn cho usage/quota hoặc burst protection.

Redis không phải source of truth. Nếu Redis chưa có hoặc tạm thời lỗi, hệ thống core vẫn phải dựa trên PostgreSQL, Kafka và OpenSearch.

### 5.8. Docker Compose

Docker Compose là môi trường local development chính.

Trách nhiệm chính:

- Chạy PostgreSQL.
- Chạy Kafka.
- Chạy OpenSearch.
- Chạy Control API.
- Chạy Ingestion API.
- Chạy Log Processor.
- Khởi tạo Kafka topics cần thiết cho local stack.
- Hỗ trợ developer kiểm tra end-to-end flow trong môi trường local.

## 6. Control Plane Và Data Plane

TraceFlow tách Control Plane và Data Plane để giữ ranh giới trách nhiệm rõ ràng.

### 6.1. Control Plane

Control Plane trả lời câu hỏi:

```text
Ai được phép làm gì, trong workspace/project/application nào?
```

Bao gồm:

- Identity.
- JWT authentication.
- Workspace/project/application management.
- Membership và RBAC.
- API key lifecycle.
- Internal API key validation.
- Search API với project access check.

Control Plane ưu tiên correctness, security và consistency của business metadata.

### 6.2. Data Plane

Data Plane trả lời câu hỏi:

```text
Log đi vào hệ thống, được xử lý và index như thế nào?
```

Bao gồm:

- Log ingestion.
- API key authentication trên ingestion path.
- Payload validation.
- Tenant context enrichment.
- Kafka publishing.
- Kafka consuming.
- Batch processing.
- Bulk indexing.
- Retry và DLQ.

Data Plane ưu tiên throughput, latency, reliability và khả năng xử lý bất đồng bộ.

## 7. Main Data Flows

### 7.1. Resource Setup Flow

```text
User
-> Control API
-> PostgreSQL
```

Luồng này bao gồm đăng ký, đăng nhập, tạo workspace, tạo project, tạo trace application và tạo API key.

Kết quả của luồng này là hệ thống có đầy đủ resource hierarchy và API key để client application gửi log.

### 7.2. API Key Creation Flow

```text
Project Manager
-> Control API
-> Generate API Key
-> Hash Secret
-> Store API Key Metadata in PostgreSQL
-> Return Full Secret Once
```

Control API chỉ trả full API key secret một lần khi tạo. Sau đó hệ thống chỉ lưu secret hash và metadata như prefix, status, expiration, environment và last used time.

### 7.3. Log Ingestion Flow

```text
Client Application
-> Ingestion API
-> Extract API Key
-> Validate API Key with Control API
-> Receive Tenant Context
-> Validate Log Payload
-> Enrich Log Event
-> Publish Kafka Event
-> Return Accepted Response
```

Ingestion API không tin workspace, project, application hoặc environment do client gửi lên. Tenant context hợp lệ luôn đến từ Control API sau khi API key được validate.

### 7.4. Log Processing Flow

```text
Kafka
-> Log Processor
-> Parse Event
-> Validate Internal Event
-> Normalize Event
-> Buffer Batch
-> Flush Batch
-> OpenSearch Bulk Index
```

Log Processor index log theo batch để giảm số request đến OpenSearch và tăng throughput.

### 7.5. Failure Và DLQ Flow

```text
Malformed JSON
-> DLQ

Invalid Internal Event
-> DLQ

Full Bulk Indexing Failure
-> Retry
-> DLQ Whole Batch if Retry Exhausted

Partial Bulk Indexing Failure
-> DLQ Failed Items Only
```

DLQ event phải lưu đủ thông tin như failure stage, failure reason, original payload, source topic, partition, offset và tenant context nếu có thể parse được.

### 7.6. Log Search Flow

```text
User
-> Control API
-> JWT Authentication
-> Project Access Check
-> Build Scoped OpenSearch Query
-> OpenSearch
-> Return Scoped Result
```

Search API luôn filter theo workspace ID và project ID. User không được query OpenSearch trực tiếp.

## 8. Storage Design Overview

| Storage | Vai trò | Dữ liệu lưu trữ |
| :--- | :--- | :--- |
| PostgreSQL | Source of truth cho control data. | User, workspace, project, application, membership, invitation, API key metadata, refresh token. |
| Kafka | Event stream và buffer tạm thời. | Enriched log events, DLQ events. |
| OpenSearch | Search storage cho log. | Enriched log documents phục vụ search, filtering và analytics cơ bản. |
| Redis | Supporting cache/counter nếu được bật. | API key validation cache, rate limiting counter, usage counter ngắn hạn. |
| Docker Volumes | Local persistence. | Dữ liệu local của PostgreSQL và OpenSearch trong môi trường development. |

## 9. Security Boundary Overview

TraceFlow có ba nhóm authentication boundary chính:

| Boundary | Cơ chế | Áp dụng cho |
| :--- | :--- | :--- |
| User-facing API | JWT access token. | User gọi Control API. |
| Ingestion API | API key. | Client Application gửi log. |
| Service-to-service | Internal service secret hoặc cơ chế tương đương. | Ingestion API gọi internal endpoint của Control API. |

Nguyên tắc bảo mật chính:

- API key không dùng để gọi management/search API.
- JWT không dùng để gửi log thay cho application.
- API key secret không lưu plaintext.
- Client không được quyết định tenant context.
- Search API luôn enforce project access trước khi query OpenSearch.
- OpenSearch không expose trực tiếp cho user hoặc client application.

## 10. Scalability Overview

### 10.1. Ingestion API

Ingestion API có thể scale ngang vì không giữ business state dài hạn. State quan trọng nằm ở Control API/PostgreSQL và Kafka.

Khi cần tối ưu, Ingestion API có thể dùng Redis để cache API key validation hoặc rate limit theo API key.

### 10.2. Kafka

Kafka hỗ trợ tăng throughput thông qua partitioning. Partitioning strategy sẽ được chốt trong tài liệu Reliability Design hoặc Scalability Design.

Kafka giúp tách tốc độ nhận log khỏi tốc độ index log.

### 10.3. Log Processor

Log Processor có thể scale bằng Kafka consumer group. Nhiều instance processor có thể consume các partition khác nhau.

Processor throughput phụ thuộc vào:

- Kafka partition count.
- Batch size.
- Flush interval.
- OpenSearch bulk indexing capacity.
- Retry và DLQ behavior.

### 10.4. OpenSearch

OpenSearch có thể scale theo index, shard và node nếu cần. Trong phạm vi local development, OpenSearch chạy single-node.

OpenSearch mapping phải hỗ trợ exact match filtering cho tenant fields và range query cho timestamp.

### 10.5. Control API

Control API có thể scale ngang, nhưng PostgreSQL vẫn là source of truth cho business metadata.

Control API cần được bảo vệ khỏi internal validation traffic quá lớn bằng cache, rate limiting hoặc circuit breaker nếu hệ thống mở rộng.

## 11. Reliability Overview

TraceFlow dùng Kafka để decouple ingestion và indexing. Khi OpenSearch tạm thời lỗi, Ingestion API vẫn có thể nhận log nếu Kafka publish thành công.

Các nguyên tắc reliability chính:

- Ingestion API chỉ trả success sau khi publish Kafka thành công.
- Invalid request bị reject trước khi vào Kafka.
- Invalid Kafka message không được làm nghẽn consumer loop.
- Log Processor retry full bulk indexing failure.
- Log Processor gửi failed event vào DLQ khi không thể xử lý thành công.
- Partial bulk failure chỉ DLQ item failed.
- DLQ event lưu đủ context để debug hoặc replay thủ công trong tương lai.
- Processor logs phải phân biệt success, retry, full failure và partial failure.

## 12. Design Decisions

### 12.1. Tách Control API Và Ingestion API

Control API xử lý business logic và user-facing API. Ingestion API xử lý log submission tốc độ cao.

Việc tách hai service giúp:

- Giữ ingestion path nhẹ và tập trung.
- Không để high-throughput log ingestion làm phức tạp Control API.
- Cho phép Data Plane scale độc lập với Control Plane.
- Giữ security boundary rõ ràng giữa user management và log ingestion.

### 12.2. Sử Dụng Kafka Giữa Ingestion Và Processing

Kafka được dùng để decouple ingestion khỏi indexing.

Lý do:

- Ingestion API không phải chờ OpenSearch.
- Hệ thống chịu traffic spike tốt hơn.
- Processor có thể retry hoặc tạm chậm mà không làm client-facing ingestion path phụ thuộc trực tiếp.
- Có thể scale processing bằng consumer group.

### 12.3. Sử Dụng OpenSearch Cho Log Search

OpenSearch phù hợp cho log search vì hỗ trợ:

- Full-text search.
- Filtering theo structured fields.
- Time range query.
- Sorting và pagination theo timestamp.
- Aggregation cơ bản cho dashboard hoặc operational insights.

PostgreSQL không được dùng làm storage chính cho high-throughput log search.

### 12.4. Sử Dụng PostgreSQL Cho Control Data

PostgreSQL được dùng cho business metadata vì dữ liệu control cần consistency, relationship và transaction rõ ràng.

Các dữ liệu như user, workspace, project, membership, application và API key metadata thuộc PostgreSQL.

### 12.5. Sử Dụng Go Cho Data Plane

Go phù hợp cho Ingestion API và Log Processor vì:

- Runtime nhẹ.
- Concurrency model tốt.
- Phù hợp với network service và worker.
- Dễ triển khai service nhỏ, độc lập.

### 12.6. Redis Là Supporting Infrastructure

Redis có thể được dùng cho cache, rate limiting và counter ngắn hạn, nhưng không phải source of truth.

Quyết định này giúp core architecture không phụ thuộc Redis trong giai đoạn đầu, nhưng vẫn có hướng mở rộng rõ ràng khi cần tối ưu ingestion path hoặc usage/quota.
