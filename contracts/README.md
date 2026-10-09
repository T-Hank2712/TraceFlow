# Contracts

Thư mục này mô tả các contract chính của TraceFlow. Contract là phần giao kèo giữa các service, giúp từng service có thể phát triển độc lập nhưng vẫn thống nhất về API, event và dữ liệu tìm kiếm.

Các contract trong thư mục này phản ánh trạng thái hiện tại của hệ thống:

- `openapi/`: contract cho Control API, Ingestion API và internal API giữa service.
- `kafka/`: schema log event được Ingestion API publish và Log Processor consume.
- `opensearch/`: cấu trúc log document được index và query trong OpenSearch.

## Nguyên tắc chung

- Contract phải được cập nhật khi thay đổi route, request body, response body, header, event field hoặc document field.
- Thay đổi tương thích ngược nên ưu tiên dạng thêm field optional.
- Thay đổi phá vỡ contract cần có kế hoạch migration cho producer, consumer và dữ liệu đã index.
- Không đưa secret, token thật, API key thật hoặc dữ liệu nhạy cảm vào contract.
- Tên field trong contract phải khớp với code, đặc biệt là field JSON gửi qua HTTP, Kafka và OpenSearch.

## Ranh giới service

- Client quản trị gọi Control API bằng JWT Bearer token.
- Client gửi log gọi Ingestion API bằng `Authorization: ApiKey <api-key>`.
- Ingestion API gọi Control API qua internal endpoint để validate API key bằng `X-Internal-Secret`.
- Ingestion API publish log event vào Kafka.
- Log Processor consume Kafka event và index log vào OpenSearch.
- Control API query OpenSearch để phục vụ màn hình tìm kiếm log và kiểm tra dữ liệu log liên quan khi xóa application.
