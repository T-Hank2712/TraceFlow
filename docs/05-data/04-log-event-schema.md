# Log Event Schema

Schema nội bộ được publish vào Kafka sau khi ingestion validate API key.

```json
{
  "EventId": "01J...",
  "BatchID": "01J...",
  "WorkspaceId": "01J...",
  "ProjectId": "01J...",
  "ApplicationId": "01J...",
  "Environment": "production",
  "Timestamp": "2026-08-15T13:30:00Z",
  "Level": 4,
  "Service": "payment-service",
  "Message": "Payment callback failed",
  "TraceId": "6fa12abc",
  "CorrelationId": "order-123",
  "Metadata": {
    "orderId": "KT001"
  }
}
```

## Nguyên tắc

Event trong Kafka phải đủ thông tin để processor index mà không cần tin lại dữ liệu tenant từ client.

Producer đang serialize `EnrichedLogEvent` bằng `System.Text.Json` mặc định, nên payload dùng PascalCase. `ReceivedAt` cũng được producer gửi, nhưng processor hiện không giữ field này khi deserialize sang `LogEvent` để index.
