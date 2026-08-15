# Coding Standards

## Nguyên tắc chung

- Code rõ ràng hơn clever.
- Không thêm abstraction nếu chưa giảm complexity thật.
- Error phải được xử lý rõ, không silent catch.
- Structured logging cho service code.
- Test đi kèm logic quan trọng.

## Go

- Context propagation cho request và Kafka operation.
- Timeout rõ ràng cho network call.
- Graceful shutdown cho HTTP server và consumer.

## C#

- Validation ở boundary.
- Authorization policy rõ.
- Database constraint bảo vệ invariant.
- Không để controller chứa business logic phức tạp.
