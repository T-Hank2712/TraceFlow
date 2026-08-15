# Error Response

TraceFlow dùng format lỗi thống nhất.

## Format

```json
{
  "code": "validation_error",
  "message": "Dữ liệu gửi lên không hợp lệ.",
  "details": {
    "field": "level"
  }
}
```

## Nhóm lỗi

- `validation_error`
- `unauthorized`
- `forbidden`
- `not_found`
- `conflict`
- `rate_limited`
- `service_unavailable`
- `internal_error`

## Nguyên tắc

Không trả stack trace cho client. Log nội bộ phải chứa đủ thông tin để debug.
