# Reliability Design

## 1. Mục Tiêu Reliability

Reliability Design mô tả cách TraceFlow phản ứng khi input lỗi, dependency chậm, Kafka/OpenSearch/Redis gặp sự cố hoặc một phần batch index thất bại. Tài liệu này không cố biến TraceFlow thành hệ thống production-grade multi-region, mà chốt reliability vừa đủ cho core scope:

```text
Multi-tenant log ingestion and search pipeline
```

Mục tiêu chính là đảm bảo log đã được hệ thống accepted không bị mất silently. Nếu log không thể index vào OpenSearch, hệ thống phải có retry hoặc DLQ record đủ thông tin để debug. Bad message không được làm nghẽn pipeline. Redis được dùng để tối ưu và bảo vệ ingestion path, nhưng không được làm sai source of truth.

Reliability trong TraceFlow dựa trên một nguyên tắc quan trọng:

```text
Accepted != Indexed
Accepted = Kafka đã nhận event
Indexed = OpenSearch đã lưu document thành công
```

Vì ingestion và indexing được xử lý bất đồng bộ, search là eventually consistent. Client gửi log không nên chờ OpenSearch, nhưng hệ thống phải có cơ chế xử lý rõ ràng nếu indexing thất bại.

## 2. Reliability Scope

Reliability scope phải bám theo scope sản phẩm đã chốt. Core pipeline cần được thiết kế kỹ vì đó là phần chứng minh năng lực backend chính của TraceFlow. Các capability nice-to-have chỉ cần reliability ở mức không phá core flow.

| Scope | Reliability Focus |
| :--- | :--- |
| Must-have | Kafka publish, processor batching, OpenSearch bulk indexing, retry, DLQ, Redis cache/rate limit behavior, search failure handling. |
| Nice-to-have | Retention job failure, quota counter drift, operational insights query failure, benchmark evidence. |
| Optional | Alerting, backup/restore và advanced dashboard không nằm trong reliability design của core. |

Với core scope, hệ thống cần thiết kế rõ ba điểm: khi nào request được xem là accepted, khi nào message đi DLQ, và khi nào dependency failure được retry hoặc trả lỗi cho caller.

## 3. Failure Taxonomy

TraceFlow phân loại lỗi theo stage xử lý thay vì chỉ theo exception cụ thể. Cách này giúp mỗi service có behavior ổn định và dễ test hơn.

| Failure Group | Ví dụ | Service xử lý chính |
| :--- | :--- | :--- |
| Client/input failure | Missing API key, invalid JSON request, invalid log level, batch quá lớn. | Ingestion API |
| Authentication failure | API key sai, revoked, expired hoặc thuộc resource archived/deleted. | Control API / Ingestion API |
| Redis failure | Cache timeout, cache miss, rate limit counter lỗi. | Control API / Ingestion API |
| Kafka failure | Publish timeout, topic unavailable, broker unavailable. | Ingestion API |
| Processor input failure | Kafka message malformed, internal event thiếu tenant field. | Log Processor |
| OpenSearch full failure | Bulk request timeout, connection error, response không có item result đáng tin cậy. | Log Processor |
| OpenSearch partial failure | Bulk request thành công nhưng một số item lỗi. | Log Processor |
| DLQ failure | Không publish được DLQ event. | Log Processor |
| Search failure | OpenSearch query timeout hoặc unavailable. | Control API |

Điểm quan trọng là không phải lỗi nào cũng retry giống nhau. Invalid input không retry. Full downstream failure có thể retry. Partial failure phải xử lý theo item để không làm sai kết quả.

## 4. Ingestion Reliability

Ingestion API nằm trên request path của client application, nên reliability ở đây phải ưu tiên phản hồi rõ ràng và không tạo cảm giác sai cho client. Ingestion API chỉ trả `202 Accepted` sau khi event hợp lệ đã được Kafka accept theo publish policy. Response này không khẳng định log đã search được.

```text
Client Application
-> Ingestion API
-> Validate API Key
-> Validate Payload
-> Rate Limit Check
-> Publish Kafka
-> 202 Accepted
```

### 4.1. Success Boundary

Boundary thành công của ingestion là Kafka publish thành công. Nếu Kafka không khả dụng hoặc publish result không xác định, Ingestion API không được trả accepted như thể event đã an toàn vào pipeline.

| Condition | Response | Reason |
| :--- | :--- | :--- |
| API key hợp lệ, payload hợp lệ, Kafka publish thành công. | `202 Accepted` | Event đã vào pipeline. |
| Payload sai format hoặc field không hợp lệ. | `400 Bad Request` | Lỗi từ client, không đưa vào Kafka. |
| API key thiếu/sai/revoked/expired. | `401 Unauthorized` | Credential không hợp lệ, không đưa vào Kafka. |
| Rate limit vượt ngưỡng. | `429 Too Many Requests` | Bảo vệ ingestion path. |
| Kafka publish thất bại rõ ràng. | `503 Service Unavailable` | Event chưa được đảm bảo vào pipeline. |

### 4.2. Batch Ingestion Policy

Batch ingestion sử dụng all-or-nothing validation trước khi publish. Nếu một item trong batch không hợp lệ, cả batch bị reject trước khi publish Kafka. Quyết định này giúp contract phía client đơn giản và tránh tình trạng client phải suy luận item nào đã vào pipeline.

```text
Batch request
-> validate API key once
-> validate whole batch
-> if any item invalid: reject whole batch
-> if all valid: publish events
```

Partial Kafka publish trong một batch là trường hợp khó hơn vì một số event có thể đã vào Kafka trước khi publish lỗi xảy ra. Thiết kế mục tiêu là Kafka publisher cần expose kết quả publish đủ rõ để Ingestion API không báo accepted sai. Trong implementation phase đầu, có thể chọn publish batch theo cách tuần tự và trả `503` nếu bất kỳ publish nào fail, đồng thời log rõ số lượng publish thành công để debug. Khi cần chặt hơn, có thể bổ sung idempotency key hoặc producer transaction, nhưng đó không phải yêu cầu core ban đầu.

### 4.3. Redis Behavior In Ingestion

Redis được dùng để cache validation hoặc rate limit, nhưng không phải source of truth.

| Redis Use Case | Failure Policy | Design Rationale |
| :--- | :--- | :--- |
| API key validation cache | Fallback sang Control API validation. | Không reject nhầm client chỉ vì cache lỗi. |
| Rate limit counter | Core/local-dev ưu tiên fail-open có warning log. | Tránh false reject khi Redis lỗi; strict mode có thể thiết kế sau. |
| Usage/quota counter | Degrade và log warning. | Quota là nice-to-have, không được phá core ingestion. |

Nếu Redis cache stale sau khi API key bị revoke, TTL ngắn và invalidation khi revoke sẽ giới hạn rủi ro. Redis cache không được lưu raw secret hoặc secret hash.

## 5. Kafka Reliability

Kafka là reliability boundary giữa ingestion và processing. Khi event đã vào Kafka, Ingestion API có thể trả accepted và để Log Processor xử lý indexing bất đồng bộ.

Kafka reliability của TraceFlow có ba mục tiêu: hấp thụ traffic spike, không để OpenSearch ảnh hưởng trực tiếp tới client request, và cho phép processor xử lý lại event khi downstream lỗi.

### 5.1. Topic Design

| Topic | Purpose |
| :--- | :--- |
| `traceflow.logs` | Main topic chứa enriched log events hợp lệ từ Ingestion API. |
| `traceflow.logs.dlq` | Dead Letter Queue chứa event/message không xử lý được. |

Main topic và DLQ topic phải tách riêng để bad message không trộn với luồng xử lý chính.

### 5.2. Partition Key Strategy

Partition key cần cân bằng giữa ordering và throughput. Với TraceFlow, ordering tuyệt đối toàn hệ thống không cần thiết. Ordering hữu ích nhất ở phạm vi application hoặc project khi debug log.

Thiết kế đề xuất:

```text
partition key = applicationId
```

Lý do:

| Option | Ưu điểm | Hạn chế |
| :--- | :--- | :--- |
| `workspaceId` | Gom theo tenant lớn. | Dễ tạo hotspot nếu workspace có nhiều log. |
| `projectId` | Giữ locality ở project. | Project lớn vẫn có thể hotspot. |
| `applicationId` | Cân bằng tốt hơn và vẫn giữ ordering tương đối theo application. | Không giữ ordering toàn project. |
| `eventId` | Phân phối đều nhất. | Mất ordering theo application/project. |

Vì TraceFlow tập trung vào log ingestion/search, không phải event sourcing yêu cầu ordering tuyệt đối, `applicationId` là lựa chọn hợp lý cho core design.

### 5.3. Poison Message Handling

Poison message là message khiến consumer xử lý lỗi lặp lại mãi. Log Processor không được để poison message block partition vô hạn. Nếu message không parse được JSON hoặc vi phạm internal schema, processor đưa message vào DLQ với failure stage phù hợp rồi tiếp tục xử lý message tiếp theo.

```text
Malformed Kafka message
-> DLQ stage json_decode
-> continue consumer loop
```

## 6. Processor Reliability

Log Processor là nơi reliability phức tạp nhất vì nó phải xử lý cả malformed message, invalid internal event, batch buffer, OpenSearch bulk failure và DLQ.

Processor flow mục tiêu:

```text
Kafka message
-> decode
-> validate
-> buffer
-> flush by size/interval
-> OpenSearch bulk index
-> success / retry / DLQ
```

### 6.1. Message-Level Failure

Message-level failure xảy ra trước khi event vào batch buffer.

| Failure | Behavior |
| :--- | :--- |
| JSON decode fail | Publish DLQ với stage `json_decode`. |
| Thiếu tenant field bắt buộc | Publish DLQ với stage `validation`. |
| Log level/timestamp/internal schema sai | Publish DLQ với stage `validation`. |

Các lỗi này không retry vì retry cùng payload sẽ không làm payload hợp lệ hơn.

### 6.2. Batch Flush Policy

Batch được flush theo hai điều kiện:

| Condition | Meaning |
| :--- | :--- |
| Batch size reached | Tăng throughput bằng cách gom đủ số event cấu hình. |
| Flush interval elapsed | Giới hạn thời gian event nằm trong buffer để giảm searchable latency. |

Mỗi item trong batch phải giữ event đã normalize và Kafka context gốc, bao gồm topic, partition, offset và raw payload. Điều này là bắt buộc để partial failure có thể DLQ đúng item lỗi.

### 6.3. OpenSearch Bulk Result Handling

Bulk indexing có ba kết quả logic:

| Result | Meaning | Processor Behavior |
| :--- | :--- | :--- |
| Success | Tất cả item index thành công. | Log indexed count và hoàn tất batch. |
| Full failure | Bulk request fail toàn bộ hoặc không có item-level result đáng tin cậy. | Retry toàn batch theo retry policy. Nếu hết retry, DLQ toàn batch. |
| Partial failure | Bulk request có item-level result nhưng một số item fail. | Chỉ DLQ item lỗi, không DLQ item thành công. |

Partial failure không được xử lý như full failure. Nếu đưa toàn batch vào DLQ khi chỉ một item lỗi, hệ thống sẽ tạo DLQ sai cho những log đã index thành công.

## 7. Retry Policy

Retry chỉ có ý nghĩa với lỗi tạm thời. Retry không dùng cho invalid input hoặc schema violation.

| Target | Retry? | Policy Direction |
| :--- | :--- | :--- |
| Control API validation | Có giới hạn | Retry ngắn cho lỗi network/5xx, không retry invalid key. |
| Kafka publish | Có giới hạn | Retry ngắn trước khi trả `503`. |
| Redis cache | Không critical | Fallback hoặc degrade theo use case. |
| OpenSearch full bulk failure | Có | Exponential backoff, max attempts cấu hình được. |
| OpenSearch partial failure | Không retry toàn batch trong core | DLQ failed items trực tiếp. |
| DLQ publish failure | Có | Retry ngắn, sau đó log critical và tránh commit success sai. |

Thiết kế core chọn policy đơn giản cho partial bulk failure:

```text
Partial bulk failure -> DLQ failed items only
```

Không retry item-level trong core để tránh tăng độ phức tạp. Nếu sau này benchmark hoặc lỗi thực tế cho thấy nhiều partial failure do lỗi tạm thời, item-level retry có thể được thêm ở Reliability Design phiên bản sau.

## 8. DLQ Design

DLQ là nơi lưu bằng chứng cho event không thể xử lý thành công. Mục tiêu của DLQ không chỉ là tránh nghẽn pipeline, mà còn giúp debug chính xác.

DLQ event cần có:

| Field Group | Required Data |
| :--- | :--- |
| Failure metadata | `failureStage`, `failureReason`, `failedAt`. |
| Kafka context | `sourceTopic`, `sourcePartition`, `sourceOffset`. |
| Event identity | `eventId` nếu parse được. |
| Tenant context | `workspaceId`, `projectId`, `applicationId` nếu parse được. |
| Payload | Raw payload hoặc event payload liên quan. |

### 8.1. Failure Stages

| Failure Stage | Khi dùng |
| :--- | :--- |
| `json_decode` | Kafka message không parse được JSON. |
| `validation` | Event parse được nhưng vi phạm internal contract. |
| `indexing` | Event hợp lệ nhưng OpenSearch indexing thất bại theo policy. |

### 8.2. DLQ Publish Failure

DLQ publish failure là lỗi nghiêm trọng vì hệ thống không thể chứng minh message lỗi đã được xử lý an toàn. Thiết kế mục tiêu:

```text
DLQ publish fails
-> retry short policy
-> if still fails, log critical with source topic/partition/offset
-> do not mark message as successfully handled
```

Advanced manual offset management nằm ngoài scope ban đầu, nhưng nguyên tắc thiết kế vẫn là không được silently drop message khi cả xử lý chính và DLQ đều thất bại.

## 9. Redis Reliability

Redis nằm trong must-have infrastructure nhưng không phải source of truth. Reliability của Redis được thiết kế theo hướng degrade an toàn.

### 9.1. Validation Cache

Validation cache giúp giảm tải cho Control API/PostgreSQL. Cache entry cần TTL ngắn và không chứa secret nhạy cảm. Khi API key bị revoke, Control API nên invalidate cache nếu có thể.

Nếu Redis unavailable hoặc cache miss, Ingestion API gọi Control API validation trực tiếp. Điều này giúp ingestion path vẫn đúng về mặt security, dù có thể chậm hơn.

### 9.2. Rate Limiting

Rate limiting dùng Redis counter để bảo vệ ingestion path trước burst traffic. Với core/local-dev scope, policy mặc định nên là fail-open kèm warning log khi Redis lỗi. Lý do là rate limiting nhằm bảo vệ hệ thống, nhưng Redis lỗi không nên tự động làm tất cả client hợp lệ bị reject.

Nếu sau này quota trở thành strict requirement, hệ thống có thể bổ sung strict mode:

```text
strict quota mode -> Redis failure can fail-closed
```

Strict mode không thuộc core ban đầu.

### 9.3. Counter Drift

Redis counter có thể lệch khi restart hoặc expire. Điều này chấp nhận được vì counter trong scope hiện tại chỉ phục vụ bảo vệ ngắn hạn và quota nice-to-have, không phục vụ billing.

## 10. Search Reliability

Search API đọc dữ liệu từ OpenSearch thông qua Control API. Search có thể lỗi vì OpenSearch unavailable, query quá rộng hoặc timeout.

Control API phải xử lý search failure an toàn:

| Failure | Behavior |
| :--- | :--- |
| OpenSearch timeout | Trả `503` hoặc error response an toàn, không expose raw stack trace. |
| Query quá rộng | Áp dụng default time range, pagination và max limit. |
| User không có quyền project | Trả `403` hoặc `404` theo policy không leak resource. |
| OpenSearch trả lỗi mapping/query | Log internal error, trả response an toàn cho user. |

Search là eventually consistent. Nếu log vừa được accepted nhưng chưa index, Search API có thể chưa trả log đó ngay.

## 11. Reliability Observability

Reliability chỉ có giá trị khi có đủ log để biết hệ thống đang xử lý đúng hay sai. TraceFlow cần structured logs cho các event quan trọng trong pipeline.

| Event | Required Fields |
| :--- | :--- |
| `ingestion_accepted` | `apiKeyPrefix`, `workspaceId`, `projectId`, `applicationId`, `eventCount`. |
| `ingestion_rejected` | `reason`, `statusCode`, `apiKeyPrefix` nếu an toàn. |
| `kafka_publish_failed` | `topic`, `reason`, `eventCount`. |
| `redis_fallback` | `operation`, `reason`, `fallbackTarget`. |
| `batch_flush_started` | `batchSize`, `flushReason`. |
| `bulk_index_result` | `batchSize`, `indexedCount`, `failedCount`, `resultType`, `retryAttempts`. |
| `dlq_publish_result` | `failureStage`, `eventId`, `sourceTopic`, `partition`, `offset`, `status`. |
| `search_failed` | `workspaceId`, `projectId`, `reason`, `durationMs`. |

Log không được chứa full API key secret, raw credential hoặc metadata nhạy cảm không cần thiết.

## 12. Reliability Acceptance Criteria

Reliability design được xem là đạt khi các behavior sau được chứng minh bằng test, manual verification hoặc log evidence.

| Scenario | Expected Result |
| :--- | :--- |
| Client gửi invalid JSON vào Ingestion API. | Request bị reject, không publish Kafka. |
| Client dùng revoked/expired API key. | Request bị reject, không publish Kafka. |
| Kafka publish fail. | Ingestion API không trả accepted sai. |
| Kafka message malformed. | Processor gửi DLQ stage `json_decode` và tiếp tục consumer loop. |
| Internal event thiếu tenant field. | Processor gửi DLQ stage `validation`. |
| OpenSearch full bulk failure. | Processor retry rồi DLQ toàn batch nếu hết retry. |
| OpenSearch partial bulk failure. | Processor chỉ DLQ failed items. |
| Redis validation cache lỗi. | Hệ thống fallback Control API, không làm sai tenant ownership. |
| Redis rate limit lỗi trong core mode. | Hệ thống fail-open có warning log. |
| DLQ publish fail. | Hệ thống retry, log critical và không silently drop message. |
| Search gặp OpenSearch timeout. | Control API trả lỗi an toàn, không expose raw OpenSearch error. |

Tài liệu này là nền để `07-testing-strategy.md` xây dựng test case cho success path, retry path, full failure path, partial failure path, Redis fallback và DLQ behavior.
