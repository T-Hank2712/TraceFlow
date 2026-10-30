# Ingestion API

Ingestion API do ASP.NET Core Ingestion API cung cấp.

## Authentication

Client gửi API key qua header:

```text
Authorization: ApiKey <secret>
```

API key được kiểm tra trạng thái revoke trước khi dùng tenant context cache. Khi Control API revoke key, một revoke marker được ghi vào Redis; các request sau đó bị từ chối thay vì tiếp tục dùng context đã cache.

Pre-auth rate limit được phân theo client IP. `X-Forwarded-For` chỉ được sử dụng khi request đi qua network proxy đã khai báo trong `TrustedProxies:KnownNetworks`; header từ client không tin cậy không thể tự chọn IP bucket.

## Single log

```text
POST /v1/logs
```

Nhận một log object và trả `200 OK` nếu đã publish vào Kafka thành công.

## Batch logs

```text
POST /v1/batch-logs
```

Nhận danh sách log. Batch có giới hạn số lượng và dung lượng; từng item được validate/publish độc lập và response trả accepted/rejected theo item. Nếu Kafka không xác nhận một hay nhiều item, API trả `503 Service Unavailable` cùng kết quả từng item.

`200 OK` chỉ được trả khi mọi item hợp lệ đã được Kafka xác nhận. Item invalid được phản ánh trong `results` và không làm các item hợp lệ đã publish bị mất.

## Idempotency khi retry

Mọi request ghi log phải có header `Idempotency-Key` không rỗng, dài tối đa 128 ký tự. Client phải giữ nguyên header và payload khi retry một request bị timeout hoặc nhận `503`.

Ingestion lưu ánh xạ API key + idempotency key + payload trong Redis trong 24 giờ. Retry hợp lệ nhận lại cùng `eventId`; Log Processor dùng `eventId` làm OpenSearch document ID nên bản ghi trùng ở Kafka ghi đè cùng document thay vì tạo log mới. Dùng lại key với payload khác trả `409 Conflict`.

Nếu Redis idempotency không sẵn sàng, API trả `503` để client không retry mù và tạo event ID logic khác.

## Metadata an toàn

`metadata` nhận object JSON nhưng có giới hạn số key, độ sâu, kích thước key/string và độ dài array theo cấu hình `Ingestion__MaxMetadata*`. Log Processor lưu metadata vào OpenSearch dưới kiểu `flat_object`, không tạo field mapping động cho từng key client gửi lên.

## Nguyên tắc

Client không được tự quyết định `workspaceId`, `projectId`, `applicationId` hoặc `environment`. Các trường tenant này được suy ra từ API key sau khi Control API validate.
