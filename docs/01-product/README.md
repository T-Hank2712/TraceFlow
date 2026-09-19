# Product

Nhóm tài liệu này mô tả TraceFlow dưới góc nhìn sản phẩm. Mục tiêu của folder này là trả lời câu hỏi: TraceFlow giải quyết vấn đề gì, phục vụ ai, luồng sử dụng chính là gì, phạm vi Core/Nice-to-have/Optional gồm những năng lực nào và nên triển khai theo thứ tự nào.

Đây là phần nên đọc đầu tiên trước khi bàn về Kafka, OpenSearch, Go hay ASP.NET Core. Nếu không hiểu rõ sản phẩm, các quyết định kỹ thuật rất dễ biến thành việc thêm công nghệ vì thích công nghệ thay vì vì nhu cầu thật.

## Nội dung cần nắm

- TraceFlow Core là multi-tenant log ingestion and search pipeline cho đội kỹ thuật.
- Người dùng chính là Backend Developer, DevOps, SRE, QA và Technical Support.
- Business flow xoay quanh Workspace, Project, Application, API Key và Log.
- Core gồm Auth/RBAC, Workspace/Project/Application, API Key, Ingestion API, Kafka, Log Processor, OpenSearch, Batch + Bulk Indexing, Retry + DLQ, Search API và Redis.
- Retention, Quota, Operational Insights, Benchmark và Minimal Web UI là nice-to-have.
- Alerting, Backup/Restore và Advanced Dashboard là optional, không phải điều kiện hoàn thiện core.

## Khi nào cần cập nhật nhóm này

- Khi thay đổi định nghĩa sản phẩm hoặc đối tượng người dùng.
- Khi thêm/bớt năng lực lớn hoặc thay đổi phân loại Core/Nice-to-have/Optional.
- Khi thay đổi business flow giữa Workspace, Project, Application và API Key.
- Khi cần giải thích vì sao một tính năng thuộc hoặc không thuộc phạm vi TraceFlow.

## Thứ tự đọc

1. `01-overview.md`
2. `02-users-and-use-cases.md`
3. `03-business-flow.md`
4. `04-complete-scope.md`
5. `05-roadmap.md`

## Kết quả mong đợi sau khi đọc

Sau khi đọc xong nhóm này, người đọc phải có thể mô tả TraceFlow trong vài phút: hệ thống phục vụ ai, luồng core đi qua đâu, phần nào là nice-to-have/optional và vì sao đây không phải một CRUD app đơn giản.
