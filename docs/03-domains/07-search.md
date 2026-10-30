# Domain: Search

Search cho phép người dùng tìm và lọc log đã được index.

## Filter

- Project.
- Application hoặc service.
- Environment.
- Level.
- Time range.
- Trace ID.
- Correlation ID.
- Message text.

## Quy tắc

- Mọi search phải bị giới hạn bởi project mà user có quyền truy cập.
- Kết quả mặc định sort theo `timestamp` giảm dần.
- Trace search nên sort theo `timestamp` tăng dần để dễ đọc flow.

## Năng lực cần có

Search core cần hỗ trợ endpoint tìm log, trace lookup theo `trace_id`, sort và pagination. Saved searches và aggregation phục vụ operational insights là nice-to-have, không phải điều kiện của core Search API.
