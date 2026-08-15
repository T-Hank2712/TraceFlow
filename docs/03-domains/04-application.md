# Domain: Application

Application đại diện cho service hoặc app gửi log vào TraceFlow.

## Entity chính

- Application.
- Project.
- API Key.

## Quy tắc

- Application thuộc đúng một project.
- Một project có thể có nhiều application.
- API key được cấp cho application.
- Log gửi bằng API key của application nào sẽ được gắn `application_id` tương ứng.

## Năng lực cần có

Application cần hỗ trợ tạo, cập nhật, vô hiệu hóa, liệt kê theo project và liên kết với API key, metrics, alert rule.
