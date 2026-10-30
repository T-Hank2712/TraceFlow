# .NET Log Processor

.NET Log Processor consume log event từ Kafka, xử lý batch và index vào OpenSearch.

## Trách nhiệm

- Consume Kafka theo consumer group.
- Validate internal event.
- Normalize các field cần index; processor hiện không thêm `processedAt`.
- Gom batch theo size hoặc interval.
- Bulk index vào OpenSearch.
- Retry, DLQ và graceful shutdown.
- Khởi tạo và xác minh index mapping an toàn trước khi consume.

## Nguyên tắc

Offset chỉ được commit sau khi event đã được index thành công hoặc được publish an toàn vào DLQ. Với lỗi không thể xử lý, event phải được đưa vào DLQ thay vì mất im lặng.

Processor dùng `eventId` làm OpenSearch document ID. Vì vậy Kafka có thể có bản ghi retry trùng về mặt vật lý, nhưng các retry cùng `eventId` ghi đè cùng document thay vì tạo log logic trùng.

Khi khởi động, processor yêu cầu index có `dynamic: false` và `metadata: flat_object`. Nếu mapping hiện có không tương thích, processor fail fast để tránh OpenSearch mapping explosion hoặc indexing sai.
