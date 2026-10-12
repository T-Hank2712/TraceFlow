# OpenSearch Contracts

Thư mục này mô tả contract dữ liệu log cần được lưu trong OpenSearch. Contract này là ranh giới giữa Log Processor, Control API Search API và các chức năng nghiệp vụ cần kiểm tra log đã tồn tại hay chưa.

Tên index được cấu hình bằng môi trường qua `OpenSearch__Index`.

## Log document

Document mà Control API Search API đang kỳ vọng:

```json
{
  "eventId": "01J...",
  "timestamp": "2026-09-19T10:15:30Z",
  "receivedAt": "2026-09-19T10:15:31Z",
  "workspaceId": "01J...",
  "projectId": "01J...",
  "applicationId": "01J...",
  "environment": "production",
  "service": "checkout-api",
  "level": "Error",
  "message": "Payment provider timeout",
  "traceId": "trace-7f9c2a",
  "correlationId": "order-10001",
  "metadata": {
    "orderId": "10001",
    "provider": "stripe"
  }
}
```

Ý nghĩa field:

- `eventId`: định danh duy nhất của log event.
- `timestamp`: thời điểm event xảy ra.
- `receivedAt`: thời điểm hệ thống nhận hoặc index log.
- `workspaceId`: workspace sở hữu log.
- `projectId`: project sở hữu log.
- `applicationId`: trace application sở hữu log.
- `environment`: môi trường của log.
- `service`: tên service phát sinh log.
- `level`: mức log.
- `message`: nội dung log.
- `traceId`: trace id, optional.
- `correlationId`: correlation id, optional.
- `metadata`: dữ liệu bổ sung, optional.

Lưu ý đồng bộ pipeline:

- Kafka event dùng `Level` dạng số theo `Microsoft.Extensions.Logging.LogLevel`.
- OpenSearch cần index `level` theo dạng có thể filter bằng `level.keyword` vì Control API hiện query field này.
- Kafka event có `ReceivedAt`; OpenSearch document cần giữ `receivedAt` để Control API trả đúng response.

## Query contract

Control API hiện query OpenSearch bằng các filter sau:

- `workspaceId.keyword`: bắt buộc.
- `projectId.keyword`: bắt buộc.
- `applicationId.keyword`: optional khi search, bắt buộc khi kiểm tra app có log hay chưa.
- `environment.keyword`: optional, được normalize về chữ thường ở query.
- `level.keyword`: optional, được normalize về chữ hoa ở query.
- `service.keyword`: optional.
- `traceId.keyword`: optional.
- `correlationId.keyword`: optional.
- `timestamp`: dùng range query với `from` và `to`.

Kết quả search được sort theo:

```json
[
  {
    "timestamp": {
      "order": "desc"
    }
  }
]
```

## Mapping kỳ vọng

Các field dùng để filter chính xác cần có khả năng query dạng keyword:

- `workspaceId`
- `projectId`
- `applicationId`
- `environment`
- `service`
- `level`
- `traceId`
- `correlationId`

Các field thời gian cần hỗ trợ range query và sort:

- `timestamp`
- `receivedAt`

`message` nên phục vụ search nội dung log. `metadata` là object linh hoạt, không nên phụ thuộc vào mapping cứng cho từng key metadata nếu chưa có nhu cầu nghiệp vụ rõ ràng.

## Ràng buộc nghiệp vụ

- Log là business data. Khi trace application đã từng có log trong OpenSearch, application không nên hard delete.
- Nếu OpenSearch không khả dụng trong lúc kiểm tra log để xóa application, hệ thống nên fail-safe theo hướng archive thay vì hard delete.
- Thay đổi mapping cần có kế hoạch migration hoặc reindex vì có thể ảnh hưởng Search API và các rule xóa/archive.

## Quy ước compatibility

- Không đổi tên field đang được Control API query.
- Không đổi kiểu dữ liệu của field đã index nếu chưa có kế hoạch reindex.
- Field mới nên được thêm theo hướng optional.
- Nếu cần đổi index hoặc mapping lớn, cần version index mới và chiến lược chuyển dữ liệu.
