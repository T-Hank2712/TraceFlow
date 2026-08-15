# Product

Nhóm tài liệu này mô tả TraceFlow dưới góc nhìn sản phẩm. Mục tiêu của folder này là trả lời câu hỏi: TraceFlow giải quyết vấn đề gì, phục vụ ai, luồng sử dụng chính là gì, phạm vi hoàn chỉnh gồm những năng lực nào và nên triển khai theo thứ tự nào.

Đây là phần nên đọc đầu tiên trước khi bàn về Kafka, OpenSearch, Go hay ASP.NET Core. Nếu không hiểu rõ sản phẩm, các quyết định kỹ thuật rất dễ biến thành việc thêm công nghệ vì thích công nghệ thay vì vì nhu cầu thật.

## Nội dung cần nắm

- TraceFlow là nền tảng log aggregation và observability cho đội kỹ thuật.
- Người dùng chính là Backend Developer, DevOps, SRE, QA và Technical Support.
- Business flow xoay quanh Workspace, Project, Application, API Key và Log.
- Sản phẩm hoàn chỉnh bao gồm ingestion, processing, search, dashboard, alerting, retention, usage/quota và vận hành.
- Roadmap chỉ mô tả thứ tự triển khai, không làm giảm phạm vi thiết kế.

## Khi nào cần cập nhật nhóm này

- Khi thay đổi định nghĩa sản phẩm hoặc đối tượng người dùng.
- Khi thêm/bớt năng lực lớn như alerting, quota, retention hoặc tracing.
- Khi thay đổi business flow giữa Workspace, Project, Application và API Key.
- Khi cần giải thích vì sao một tính năng thuộc hoặc không thuộc phạm vi TraceFlow.

## Thứ tự đọc

1. `01-overview.md`
2. `02-users-and-use-cases.md`
3. `03-business-flow.md`
4. `04-complete-scope.md`
5. `05-roadmap.md`

## Kết quả mong đợi sau khi đọc

Sau khi đọc xong nhóm này, người đọc phải có thể mô tả TraceFlow trong vài phút: hệ thống phục vụ ai, luồng chính đi qua đâu, sản phẩm hoàn chỉnh cần có gì và vì sao đây không phải một CRUD app đơn giản.
