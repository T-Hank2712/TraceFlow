# Test Data

Test data cần đại diện cho nhiều service, level và trace.

## Bộ dữ liệu mẫu

- Nhiều workspace.
- Nhiều project trong một workspace.
- Nhiều application trong một project.
- Log đủ level từ `TRACE` tới `FATAL`.
- Trace có nhiều bước xuyên service.
- Metadata có nhiều key nhưng không gây mapping explosion.

## Nguyên tắc

Test data phải có trường hợp cross-tenant để kiểm tra không lộ dữ liệu.
