# Domains

Nhóm tài liệu này mô tả các domain chính của TraceFlow. Mục tiêu là làm rõ các khái niệm nghiệp vụ trước khi biến chúng thành bảng database, endpoint API hoặc UI dashboard.

Domain docs trả lời câu hỏi: hệ thống có những thực thể nào, quan hệ giữa chúng ra sao, invariant nào phải được bảo vệ và hành vi nào là bắt buộc. Ví dụ API Key không chỉ là một bảng dữ liệu; nó có rule bảo mật, vòng đời, trạng thái revoke, scope theo application và ảnh hưởng trực tiếp tới ingestion.

## Nội dung cần nắm

- Identity xác định user và phiên đăng nhập.
- Workspace gom user và project theo tổ chức/đội nhóm.
- Project là boundary chính cho log và search.
- Application đại diện cho service gửi log.
- API Key là credential của application, không phải credential của user.
- Log, Search và Dashboard là phần người dùng khai thác dữ liệu.
- Alerting, Retention, Usage/Quota làm sản phẩm hoàn chỉnh và vận hành được lâu dài.

## Khi nào cần cập nhật nhóm này

- Khi thêm entity hoặc rule nghiệp vụ mới.
- Khi thay đổi quan hệ giữa Workspace, Project, Application hoặc API Key.
- Khi phát hiện invariant cần bảo vệ bằng database constraint hoặc authorization policy.
- Khi API hoặc schema thay đổi vì domain behavior thay đổi.

## Thứ tự đọc

1. `01-identity.md`
2. `02-workspace.md`
3. `03-project.md`
4. `04-application.md`
5. `05-api-key.md`
6. `06-log.md`
7. `07-search.md`
8. `08-dashboard.md`
9. `09-alerting.md`
10. `10-retention.md`
11. `11-usage-and-quota.md`

## Kết quả mong đợi sau khi đọc

Sau khi đọc xong nhóm này, người đọc phải hiểu được mô hình nghiệp vụ của TraceFlow và biết những rule nào không được phá khi implement database, API hoặc background job.
