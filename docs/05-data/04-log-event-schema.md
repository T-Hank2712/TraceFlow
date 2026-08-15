# Log Event Schema

Schema nội bộ được publish vào Kafka sau khi ingestion validate API key.

```json
{
  "event_id": "uuid",
  "timestamp": "2026-08-15T13:30:00Z",
  "received_at": "2026-08-15T13:30:01Z",
  "level": "ERROR",
  "project_id": "uuid",
  "application_id": "uuid",
  "service": "payment-service",
  "environment": "production",
  "message": "Payment callback failed",
  "trace_id": "6fa12abc",
  "correlation_id": "order-123",
  "metadata": {
    "order_id": "KT001"
  }
}
```

## Nguyên tắc

Event trong Kafka phải đủ thông tin để processor index mà không cần tin lại dữ liệu tenant từ client.
