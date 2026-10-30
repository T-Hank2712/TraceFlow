# TraceFlow Docs

Đây là bộ tài liệu chính thức của TraceFlow - nền tảng thu thập, xử lý, lưu trữ và tìm kiếm log tập trung.

## Vai trò của bộ tài liệu

Bộ docs này không chỉ là phần mô tả phụ cho code. Đây là nơi giải thích cách TraceFlow được hiểu, được sử dụng, được triển khai và được vận hành. Các quyết định thiết kế gốc nằm trong `system-design/`; `docs/` diễn giải các quyết định đó thành tài liệu sản phẩm, API, dữ liệu, service, bảo mật, vận hành và test.

TraceFlow là một hệ thống có nhiều thành phần bất đồng bộ. Vì vậy tài liệu phải giúp trả lời rõ:

- User nào dùng hệ thống và dùng để làm gì.
- Log đi qua những service nào.
- Dữ liệu được cô lập theo workspace/project ra sao.
- API contract giữa client, control plane và data plane là gì.
- Khi Kafka, OpenSearch, Redis hoặc processor lỗi thì hệ thống xử lý thế nào.
- Làm sao test, vận hành và debug local stack.

## Cách đọc tài liệu

1. Đọc `01-product/` để hiểu bài toán, người dùng và phạm vi `Core / Nice-to-have / Optional`.
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

TraceFlow Core được chốt là một multi-tenant log ingestion and search pipeline. Luồng cốt lõi của hệ thống là:

```text
Project -> Application -> API Key -> Ingestion -> Kafka -> Processor -> OpenSearch -> Search
```

Các tài liệu trong repo phải bám theo scope mới: core gồm Auth/RBAC, Workspace/Project/Application, API Key, Ingestion API, Kafka, Log Processor, OpenSearch, Batch + Bulk Indexing, Retry + DLQ, Search API và Redis. Retention, Quota, Operational Insights, Benchmark và Minimal Web UI là nice-to-have. Alerting, Backup/Restore và Advanced Dashboard là optional.
