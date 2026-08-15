# TraceFlow Docs

Đây là bộ tài liệu chính thức của TraceFlow - nền tảng thu thập, xử lý, lưu trữ và tìm kiếm log tập trung.

## Vai trò của bộ tài liệu

Bộ docs này không chỉ là phần mô tả phụ cho code. Đây là nơi chốt cách TraceFlow được hiểu, được thiết kế và được vận hành. Mọi quyết định quan trọng về sản phẩm, domain, API, dữ liệu, bảo mật, vận hành và test cần có dấu vết trong docs để khi code phát triển, project không bị lệch khỏi mục tiêu ban đầu.

TraceFlow là một hệ thống có nhiều thành phần bất đồng bộ. Vì vậy tài liệu phải giúp trả lời rõ:

- User nào dùng hệ thống và dùng để làm gì.
- Log đi qua những service nào.
- Dữ liệu được cô lập theo workspace/project ra sao.
- API contract giữa client, control plane và data plane là gì.
- Khi Kafka, OpenSearch hoặc processor lỗi thì hệ thống xử lý thế nào.
- Làm sao test, vận hành và debug local stack.

## Cách đọc tài liệu

1. Đọc `01-product/` để hiểu bài toán, người dùng và phạm vi sản phẩm hoàn chỉnh.
2. Đọc `02-architecture/` để hiểu kiến trúc tổng thể và ranh giới service.
3. Đọc `03-domains/` để nắm các domain nghiệp vụ chính.
4. Đọc `04-api/` và `05-data/` trước khi bắt đầu implement contract.
5. Đọc `06-services/`, `08-operations/`, `09-testing/` khi triển khai và vận hành local stack.

## Quy ước tổ chức

- Folder được đánh số để thể hiện thứ tự đọc ở cấp cao.
- Mỗi folder có `README.md` làm trang định hướng.
- File nội dung bên trong folder được đánh số theo thứ tự đọc trong nhóm đó.
- `00-documentation-structure.md` là bản đồ đầy đủ của toàn bộ cây docs.

## Nguyên tắc

TraceFlow được thiết kế như một nền tảng log aggregation và observability hoàn chỉnh. Luồng cốt lõi của hệ thống là:

```text
Project -> Application -> API Key -> Ingestion -> Kafka -> Processor -> OpenSearch -> Search
```

Các tài liệu trong repo mô tả trạng thái hoàn thiện của project. Khi triển khai thực tế, đội phát triển có thể chia nhỏ theo milestone, nhưng contract và kiến trúc phải hướng tới toàn bộ sản phẩm.
