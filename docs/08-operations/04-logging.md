# Logging

TraceFlow phải tự log theo structured logging.

## Trường nên có

- `timestamp`
- `level`
- `service`
- `environment`
- `request_id`
- `trace_id`
- `project_id`
- `duration_ms`
- `error`

## Nguyên tắc

Không log secret, API key, refresh token hoặc payload chứa dữ liệu nhạy cảm không cần thiết.
