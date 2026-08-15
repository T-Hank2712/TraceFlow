# Development

Nhóm tài liệu này mô tả cách phát triển TraceFlow trong repo: setup môi trường, quy chuẩn code, workflow Git, branching, commit convention và definition of done.

Development docs không thay thế architecture docs, nhưng giúp biến các quyết định thiết kế thành thói quen làm việc hằng ngày. Khi project có cả Go, C#, Docker, Kafka và OpenSearch, quy ước phát triển rõ ràng giúp giảm lỗi do mỗi phần đi theo một kiểu riêng.

## Nội dung cần nắm

- Cách chuẩn bị môi trường local.
- Quy chuẩn code chung cho Go và C#.
- Cách đặt branch và viết commit.
- Thông tin cần có trong pull request.
- Điều kiện để một thay đổi được xem là hoàn thành.

## Khi nào cần cập nhật nhóm này

- Khi thay đổi cách chạy project local.
- Khi thêm tooling, linter, formatter hoặc test runner.
- Khi thay đổi workflow branch/commit/PR.
- Khi definition of done cần bổ sung tiêu chí mới như security, docs hoặc benchmark.

## Thứ tự đọc

1. `01-setup.md`
2. `02-coding-standards.md`
3. `03-git-workflow.md`
4. `04-branching-strategy.md`
5. `05-commit-conventions.md`
6. `06-definition-of-done.md`

## Kết quả mong đợi sau khi đọc

Sau khi đọc xong nhóm này, người phát triển mới phải biết cách setup, code, test, commit và tự kiểm tra một thay đổi trước khi merge.
