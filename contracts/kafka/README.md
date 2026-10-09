# Kafka Contracts

Thư mục này mô tả Kafka event contract của TraceFlow. Kafka là ranh giới bất đồng bộ giữa Ingestion API và Log Processor.

## Luồng chính

1. Application gửi log đến Ingestion API bằng API key.
2. Ingestion API validate API key qua Control API.
3. Ingestion API gắn thông tin tenant vào log event.
4. Ingestion API publish event vào Kafka topic chính.
5. Log Processor consume event và index vào OpenSearch.
6. Event không xử lý được sẽ được đưa vào DLQ topic.

Tên topic được cấu hình bằng môi trường:

- `Kafka__Topic`: topic log chính.
- `Kafka__DlqTopic`: topic dead-letter của Log Processor.

## Log event

Producer: Ingestion API  
Consumer: Log Processor

Kafka message hiện đang dùng `System.Text.Json` mặc định, vì vậy field name trong payload là PascalCase.

Schema hiện tại được Ingestion API publish:

```json
{
  "EventId": "01J...",
  "BatchID": "01J...",
  "WorkspaceId": "01J...",
  "ProjectId": "01J...",
  "ApplicationId": "01J...",
  "Environment": "production",
  "Timestamp": "2026-09-19T10:15:30Z",
  "Level": 4,
  "Service": "checkout-api",
  "Message": "Payment provider timeout",
  "TraceId": "trace-7f9c2a",
  "CorrelationId": "order-10001",
  "Metadata": {
    "orderId": "10001",
    "provider": "stripe"
  },
  "ReceivedAt": "2026-09-19T10:15:31Z"
}
```

Ý nghĩa field:

- `EventId`: định danh duy nhất của log event.
- `BatchID`: định danh batch. Với single log, field này vẫn được sinh để thống nhất pipeline.
- `WorkspaceId`: workspace được resolve từ API key.
- `ProjectId`: project được resolve từ API key.
- `ApplicationId`: trace application được resolve từ API key.
- `Environment`: môi trường của API key, ví dụ `development`, `staging`, `production`.
- `Timestamp`: thời điểm event xảy ra ở client. Nếu client không gửi, Ingestion API dùng thời điểm nhận request.
- `Level`: mức log theo `Microsoft.Extensions.Logging.LogLevel` (`Trace=0`, `Debug=1`, `Information=2`, `Warning=3`, `Error=4`, `Critical=5`).
- `Service`: tên service phát sinh log.
- `Message`: nội dung log.
- `TraceId`: định danh trace, optional.
- `CorrelationId`: định danh correlation/request/business flow, optional.
- `Metadata`: dữ liệu bổ sung dạng key-value, optional.
- `ReceivedAt`: thời điểm Ingestion API nhận log.

Các field tenant (`WorkspaceId`, `ProjectId`, `ApplicationId`, `Environment`) không được lấy từ client request. Các field này phải được resolve từ API key đã validate.

Log Processor hiện consume các field cần thiết cho indexing (`EventId`, `WorkspaceId`, `ProjectId`, `ApplicationId`, `Environment`, `Service`, `Level`, `Message`, `Timestamp`, `TraceId`, `CorrelationId`, `Metadata`). Field mới chỉ nên thêm theo hướng không phá consumer hiện tại.

## DLQ event

Producer: Log Processor  
Consumer: tooling hoặc job xử lý lỗi trong tương lai

Schema hiện tại:

```json
{
  "Event": {
    "EventId": "01J...",
    "WorkspaceId": "01J...",
    "ProjectId": "01J...",
    "ApplicationId": "01J...",
    "Environment": "production",
    "Service": "checkout-api",
    "Level": 4,
    "Message": "Payment provider timeout",
    "Timestamp": "2026-09-19T10:15:30Z",
    "TraceId": "trace-7f9c2a",
    "CorrelationId": "order-10001",
    "Metadata": {
      "orderId": "10001"
    }
  },
  "FailureType": "OpenSearchItemFailure",
  "FailureReason": "OpenSearch failed to index the event.",
  "FailedAt": "2026-09-19T10:15:35Z"
}
```

## Quy ước compatibility

- Không đổi tên field đang được consumer sử dụng.
- Không đổi kiểu dữ liệu của field đã tồn tại.
- Field mới nên là optional hoặc có default rõ ràng ở consumer.
- Nếu cần breaking change, tạo version event mới hoặc triển khai producer/consumer theo chiến lược song song.
- Không publish secret, API key hoặc token vào Kafka.
