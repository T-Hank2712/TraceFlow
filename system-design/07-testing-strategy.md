# Testing Strategy

## 1. Mục Tiêu Kiểm Thử

Testing Strategy mô tả cách TraceFlow chứng minh core design hoạt động đúng bằng test và evidence. Tài liệu này không nhằm liệt kê mọi test nhỏ có thể viết, mà xác định những nhóm kiểm thử bắt buộc để chứng minh hệ thống đạt mục tiêu:

```text
Multi-tenant log ingestion and search pipeline
```

Testing của TraceFlow cần chứng minh bốn điều:

| Mục tiêu | Ý nghĩa |
| :--- | :--- |
| Functional correctness | Các flow chính như auth, resource setup, API key, ingestion, processing và search hoạt động đúng. |
| Tenant isolation | User/client không thể đọc hoặc ghi log sai workspace/project/application. |
| Reliability behavior | Invalid message, full failure, partial failure, Redis/Kafka/OpenSearch lỗi được xử lý đúng policy. |
| Performance evidence | Batch processing và OpenSearch Bulk API thật sự tạo ra throughput/latency có thể đo được. |

Nếu một test không chứng minh core pipeline, security boundary hoặc reliability behavior, test đó không nên được ưu tiên trong giai đoạn đầu.

## 2. Test Scope

Testing scope bám theo scope sản phẩm đã chốt. Must-have cần test kỹ vì đây là xương sống của project. Nice-to-have cần test vừa đủ khi được triển khai. Optional không bắt buộc trong core testing.

| Scope | Testing Expectation |
| :--- | :--- |
| Must-have | Unit, integration và E2E test cho auth/RBAC, API key, ingestion, Kafka, processor, OpenSearch, retry/DLQ, Redis và Search API. |
| Nice-to-have | Test theo capability khi triển khai: retention, quota, operational insights, benchmark, minimal Web UI. |
| Optional | Alerting, backup/restore và advanced dashboard không nằm trong test scope bắt buộc. |

Testing Strategy ưu tiên test behavior hơn test implementation detail. Ví dụ, test cần chứng minh “revoked API key không ingest được log”, không cần phụ thuộc trực tiếp vào tên private function xử lý revoke.

## 3. Test Pyramid

TraceFlow là hệ thống nhiều service nên không thể chỉ dựa vào unit test. Test pyramid cần kết hợp unit, integration, E2E, security và performance test.

| Test Level | Mục đích | Ví dụ |
| :--- | :--- | :--- |
| Unit Test | Kiểm tra business rule, validator, mapper, policy nhỏ. | API key expiration, log payload validation, bulk result classification. |
| Integration Test | Kiểm tra service với dependency thật hoặc gần thật. | Control API với PostgreSQL, Ingestion API với Kafka/Redis, Processor với OpenSearch. |
| Contract Test | Kiểm tra schema request/event/document không lệch giữa service. | Kafka LogEvent schema, DLQ event schema, OpenSearch document fields. |
| End-to-End Test | Kiểm tra flow xuyên service. | Create API key -> ingest log -> process -> search log. |
| Security Test | Kiểm tra auth, RBAC, tenant spoofing, secret leakage. | User không có quyền project không search được log. |
| Reliability Test | Kiểm tra failure path. | OpenSearch full failure retry rồi DLQ whole batch. |
| Performance Test | Đo throughput và latency. | logs/s, ingestion latency, searchable latency, processor throughput. |

Unit test giúp feedback nhanh, nhưng E2E và reliability test mới chứng minh TraceFlow là pipeline thật chứ không chỉ là các module rời rạc.

## 4. Test Environment Strategy

Local test environment dùng Docker Compose để chạy các dependency chính. Đây là môi trường đủ thực tế cho portfolio/core scope mà không cần Kubernetes hoặc cloud deployment.

| Dependency | Dùng trong test nào | Ghi chú |
| :--- | :--- | :--- |
| PostgreSQL | Control API integration, auth/RBAC, API key lifecycle. | Cần seed dữ liệu sạch cho mỗi test suite. |
| Redis | API key cache, rate limit, fallback behavior. | Cần test cả Redis available và unavailable. |
| Kafka | Ingestion integration, processor integration, E2E. | Cần topic main và DLQ. |
| OpenSearch | Processor indexing, Search API, benchmark. | Cần mapping rõ và cleanup index giữa test. |
| Docker Compose | E2E/performance local stack. | Môi trường chính để tái hiện pipeline. |

Test nên có cách reset state rõ ràng. PostgreSQL có thể reset bằng migration + seed. Kafka topic và OpenSearch index cần cleanup để tránh test sau bị ảnh hưởng bởi dữ liệu cũ.

## 5. Test Data Strategy

Test data phải phản ánh tenant hierarchy thật của TraceFlow. Nếu test data quá đơn giản, rất dễ bỏ sót lỗi tenant isolation.

Baseline seed nên có:

| Resource | Test Data |
| :--- | :--- |
| Users | `ownerA`, `memberA`, `outsiderB`. |
| Workspaces | `workspaceA`, `workspaceB`. |
| Projects | `projectA1`, `projectA2`, `projectB1`. |
| Applications | `appA1Production`, `appA1Staging`, `appB1Production`. |
| API Keys | active key, revoked key, expired key, key for archived resource. |
| Logs | Valid logs theo nhiều level, service, traceId, correlationId và time range. |

Data này cho phép test các tình huống quan trọng: user có quyền project A không được thấy project B, API key của app A không được gắn log sang app B, và search filter không phá tenant scope.

## 6. Control API Test Strategy

Control API cần được test như source of truth cho identity, resource, RBAC, API key lifecycle và search authorization.

### 6.1. Auth And Session Tests

| Scenario | Expected Result |
| :--- | :--- |
| User đăng nhập với credential hợp lệ. | Nhận access token và refresh token. |
| User đăng nhập sai password. | Bị từ chối bằng response an toàn. |
| Access token hết hạn. | Request user-facing API bị từ chối. |
| Refresh token bị revoke. | Không refresh được session. |

Các test này chứng minh JWT là credential hợp lệ cho Control API, nhưng không liên quan ingestion path.

### 6.2. RBAC And Resource Tests

| Scenario | Expected Result |
| :--- | :--- |
| Workspace owner tạo project. | Project được tạo trong đúng workspace. |
| User không thuộc project gọi search. | Bị từ chối. |
| Project viewer search logs. | Được phép nếu role có quyền view log. |
| User từ workspace B truy cập project A. | Bị từ chối hoặc không thấy resource. |

Mục tiêu là chứng minh quyền truy cập được enforce theo workspace/project, không chỉ theo user đã đăng nhập.

### 6.3. API Key Lifecycle Tests

| Scenario | Expected Result |
| :--- | :--- |
| Tạo API key. | Full secret chỉ xuất hiện trong create response. |
| List API keys. | Chỉ thấy metadata/prefix, không thấy full secret. |
| Validate active API key. | Trả tenant context đúng. |
| Validate revoked API key. | Trả invalid. |
| Validate expired API key. | Trả invalid. |
| Validate key thuộc archived project/application. | Trả invalid. |

Các test này là nền cho ingestion security. Nếu API key lifecycle sai, toàn bộ tenant resolution phía ingestion sẽ sai.

## 7. Ingestion API Test Strategy

Ingestion API cần chứng minh ba behavior: authentication bằng API key, validation payload, và publish Kafka event đã enrich tenant context phía server.

### 7.1. Authentication And Validation Tests

| Scenario | Expected Result |
| :--- | :--- |
| Request thiếu API key. | `401`, không publish Kafka. |
| Request dùng API key sai format. | `401`, không publish Kafka. |
| Request dùng revoked/expired key. | `401`, không publish Kafka. |
| Request thiếu `service`, `level` hoặc `message`. | `400`, không publish Kafka. |
| Request có level không hợp lệ. | `400`, không publish Kafka. |
| Request vượt body size hoặc batch size. | `413` hoặc `400`, không publish Kafka. |

Test cần assert không chỉ HTTP status, mà cả việc Kafka không nhận event khi request bị reject.

### 7.2. Tenant Enrichment Tests

| Scenario | Expected Result |
| :--- | :--- |
| Client gửi log bằng API key hợp lệ. | Kafka event có `workspaceId`, `projectId`, `applicationId`, `environment` từ Control API. |
| Client cố gửi `workspaceId/projectId/applicationId` trong payload. | Kafka event vẫn dùng tenant context từ API key, không dùng field client gửi. |
| API key production gửi log. | Event có environment đúng theo API key metadata. |

Đây là nhóm test quan trọng nhất để chứng minh client không spoof được tenant.

### 7.3. Redis Behavior Tests

| Scenario | Expected Result |
| :--- | :--- |
| Redis cache hit validation. | Ingestion dùng cached tenant context hợp lệ. |
| Redis cache miss. | Ingestion gọi Control API validation. |
| Redis unavailable khi validation cache. | Fallback Control API, request hợp lệ vẫn có thể accepted. |
| Rate limit exceeded. | Request bị trả `429`, không publish Kafka. |
| Redis rate limit unavailable trong core mode. | Fail-open có warning log theo reliability policy. |

Redis test phải chứng minh Redis không phải source of truth. Khi cache lỗi, correctness vẫn dựa vào Control API/PostgreSQL.

### 7.4. Kafka Publish Tests

| Scenario | Expected Result |
| :--- | :--- |
| Valid single log. | Publish đúng một Kafka event và trả `202`. |
| Valid batch logs. | Publish đúng số lượng event và trả accepted count. |
| Kafka unavailable. | Không trả accepted sai; trả lỗi phù hợp. |
| Batch có một item invalid. | Reject cả batch trước khi publish. |

## 8. Log Processor Test Strategy

Log Processor cần test kỹ nhất vì đây là nơi xử lý batch, bulk indexing, retry và DLQ.

### 8.1. Message Decode And Validation Tests

| Scenario | Expected Result |
| :--- | :--- |
| Kafka message không phải JSON hợp lệ. | DLQ stage `json_decode`. |
| Kafka event thiếu `workspaceId` hoặc `projectId`. | DLQ stage `validation`. |
| Kafka event thiếu `service`, `level` hoặc `message`. | DLQ stage `validation`. |
| Kafka event hợp lệ. | Được thêm vào batch buffer. |

### 8.2. Batch Flush Tests

| Scenario | Expected Result |
| :--- | :--- |
| Buffer đạt batch size. | Flush batch ngay. |
| Buffer chưa đạt size nhưng quá flush interval. | Flush theo interval. |
| Batch item được buffer. | Giữ event và Kafka context topic/partition/offset/raw payload. |

Batch flush test cần chứng minh cả throughput behavior và khả năng DLQ đúng item sau này.

### 8.3. Bulk Indexing Tests

| Scenario | Expected Result |
| :--- | :--- |
| OpenSearch bulk success. | Tất cả item được index, không có DLQ. |
| OpenSearch full bulk failure lần đầu rồi success. | Retry và index thành công, không DLQ. |
| OpenSearch full bulk failure hết retry. | Toàn batch vào DLQ stage `indexing`. |
| OpenSearch partial bulk failure. | Chỉ failed items vào DLQ; successful items không vào DLQ. |
| OpenSearch trả item-level error reason. | DLQ event có failure reason tương ứng. |

Đây là nhóm test bắt buộc để chứng minh feature batch/bulk/retry/DLQ đúng acceptance criteria.

### 8.4. DLQ Tests

| Scenario | Expected Result |
| :--- | :--- |
| DLQ từ `json_decode`. | DLQ event có raw payload và source topic/partition/offset. |
| DLQ từ `validation`. | DLQ event có parsed fields nếu có và reason rõ. |
| DLQ từ `indexing`. | DLQ event có eventId, tenant context và failure reason. |
| DLQ publish failure. | Retry, log critical và không silently drop message. |

## 9. Search API Test Strategy

Search API cần chứng minh user chỉ thấy log trong project được cấp quyền và các filter hoạt động đúng trong tenant scope.

| Scenario | Expected Result |
| :--- | :--- |
| User có quyền project search logs. | Nhận log trong project đó. |
| User không có quyền project search logs. | Bị từ chối. |
| Search theo `applicationId`. | Chỉ trả log của application đó trong project scope. |
| Search theo `environment`, `level`, `service`. | Filter đúng nhưng không vượt tenant scope. |
| Search theo `traceId` hoặc `correlationId`. | Trả đúng nhóm log liên quan. |
| Không truyền time range. | Dùng default time range hợp lý. |
| Query vượt max limit. | Bị giới hạn hoặc reject theo policy. |
| OpenSearch timeout. | Control API trả lỗi an toàn, không expose raw error. |

Search test nên seed log ở nhiều project khác nhau để phát hiện lỗi thiếu tenant filter.

## 10. Security And Tenancy Tests

Security tests kiểm tra các boundary đã thiết kế trong `06-security-and-tenancy.md`.

| Scenario | Expected Result |
| :--- | :--- |
| JWT hợp lệ nhưng user không thuộc project. | Không search được log. |
| API key của project A gửi payload chứa project B. | Event vẫn thuộc project A. |
| API key full secret sau khi tạo. | Không xuất hiện trong list/detail response. |
| Server log khi auth fail. | Không chứa full API key/JWT/internal secret. |
| Internal validation endpoint thiếu service secret. | Bị từ chối. |
| OpenSearch endpoint không public cho user/client. | Không có access path trực tiếp. |
| Redis cache chứa validation result. | Không chứa raw API key secret hoặc secret hash. |

Security tests không chỉ kiểm tra status code; chúng cần kiểm tra dữ liệu không bị leak và tenant context không bị thay đổi sai.

## 11. Reliability Tests

Reliability tests chứng minh các policy trong `05-reliability-design.md` hoạt động đúng.

| Failure Path | Test Evidence |
| :--- | :--- |
| Kafka publish failure | Ingestion không trả accepted sai. |
| Redis validation cache failure | Fallback Control API và vẫn giữ tenant đúng. |
| Redis rate limit failure | Fail-open có warning log trong core mode. |
| Malformed Kafka message | DLQ stage `json_decode`. |
| Invalid internal event | DLQ stage `validation`. |
| OpenSearch full failure | Retry attempts xuất hiện trong log và DLQ whole batch nếu exhausted. |
| OpenSearch partial failure | DLQ failed items only. |
| DLQ publish failure | Retry/log critical, không silently drop. |
| OpenSearch search timeout | Search API trả lỗi an toàn. |

Mỗi reliability test nên có assertion về side effect, ví dụ DLQ topic có đúng số message, OpenSearch không có item lỗi, processor log có `retryAttempts`.

## 12. Performance And Benchmark Tests

Benchmark không cần mô phỏng enterprise traffic, nhưng phải đủ để chứng minh batch processing và bulk indexing có giá trị.

### 12.1. Metrics

| Metric | Meaning |
| :--- | :--- |
| Ingestion throughput | Số log/s Ingestion API có thể nhận và publish Kafka. |
| Ingestion latency | Thời gian từ request đến response accepted. |
| Processor throughput | Số log/s processor index vào OpenSearch. |
| Searchable latency | Thời gian từ accepted đến khi search thấy log. |
| P95 latency | P95 của ingestion latency hoặc searchable latency theo benchmark target. |
| DLQ rate | Tỷ lệ log vào DLQ trong failure tests. |

### 12.2. Benchmark Scenarios

| Scenario | Purpose |
| :--- | :--- |
| Single log steady load | Baseline ingestion path. |
| Batch log ingestion | Đo lợi ích request batching. |
| Processor batch size comparison | So sánh throughput với batch size khác nhau. |
| Bulk indexing vs per-message indexing | Chứng minh Bulk API cải thiện throughput. |
| OpenSearch degraded scenario | Quan sát retry/DLQ behavior. |

Benchmark output nên lưu lại logs/s, P95 latency, config batch size, flush interval và resource assumptions để có thể đưa vào portfolio/CV.

## 13. Test Execution Order

Test nên được chạy theo thứ tự từ nhanh đến chậm để feedback tốt hơn.

```text
1. Unit tests
2. Contract/schema tests
3. Service integration tests
4. Processor reliability tests
5. End-to-end tests
6. Security/tenancy tests
7. Performance benchmark tests
```

Performance test không cần chạy trên mọi commit local. Nó nên chạy theo nhu cầu trước khi chốt feature hoặc trước khi lấy số liệu portfolio.

## 14. Minimum Evidence For Portfolio

Để TraceFlow đủ thuyết phục, project cần có evidence tối thiểu chứ không chỉ có code.

| Evidence | Nội dung |
| :--- | :--- |
| E2E success evidence | Create API key -> ingest log -> process -> search log. |
| Invalid input evidence | Invalid JSON/payload bị reject hoặc DLQ đúng stage. |
| Full failure evidence | OpenSearch full failure retry rồi DLQ whole batch. |
| Partial failure evidence | Chỉ failed items vào DLQ. |
| Tenant isolation evidence | User/project không có quyền không thấy log. |
| API key security evidence | Revoked/expired key không ingest được log. |
| Benchmark evidence | Throughput logs/s và P95 latency. |

Evidence có thể là test output, script output, screenshots terminal, hoặc document ghi lại command và result. Quan trọng là có thể chứng minh system design đã được kiểm chứng.

## 15. Testing Acceptance Criteria

Testing Strategy được xem là đạt khi các nhóm test sau tồn tại hoặc có kế hoạch implement rõ ràng.

| Area | Acceptance Criteria |
| :--- | :--- |
| Control API | Auth/RBAC/resource/API key lifecycle có test. |
| Ingestion API | API key auth, validation, tenant enrichment, Kafka publish và Redis fallback có test. |
| Log Processor | Decode, validation, batch flush, bulk success, full failure, partial failure và DLQ có test. |
| Search API | Project-scoped search, filters, pagination và OpenSearch failure có test. |
| Security | Tenant spoofing, cross-project access và secret leakage có test. |
| Reliability | Kafka, Redis, OpenSearch và DLQ failure path có test. |
| Performance | Có benchmark đo throughput và P95 latency khi core pipeline hoàn thiện. |

Tài liệu này sẽ dẫn sang `08-implementation-roadmap.md`, nơi các test/evidence được chia theo phase triển khai để tránh làm mọi thứ cùng lúc.
