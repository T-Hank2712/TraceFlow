# Architecture Decision Records

Thư mục này lưu các Architecture Decision Record của TraceFlow. ADR dùng để ghi lại những quyết định kỹ thuật có ảnh hưởng dài hạn, nhất là các lựa chọn nếu thay đổi sẽ tốn công hoặc làm lệch kiến trúc.

ADR không phải nơi ghi mọi chi tiết nhỏ. Nó dành cho các quyết định như chọn ngôn ngữ, chọn Kafka, chọn OpenSearch, cách chia service boundary, chiến lược multi-tenancy, retry/DLQ hoặc retention.

## Một ADR nên trả lời

- Bối cảnh lúc ra quyết định là gì.
- Quyết định cụ thể là gì.
- Vì sao chọn phương án này.
- Có phương án thay thế nào đã cân nhắc.
- Hệ quả tích cực và đánh đổi là gì.
- Quyết định này ảnh hưởng service, data, API hoặc operations nào.

## Khi nào cần cập nhật nhóm này

- Khi chọn hoặc thay thế công nghệ nền tảng.
- Khi thay đổi service boundary.
- Khi thay đổi delivery guarantee, retry, DLQ hoặc offset strategy.
- Khi thay đổi storage responsibility giữa PostgreSQL, Kafka, OpenSearch hoặc Redis.
- Khi một quyết định có đánh đổi đủ lớn để cần giải thích cho người đọc sau này.

## Quy ước

- File đặt tên dạng `adr-0001-short-title.md`.
- Một ADR chỉ ghi một quyết định.
- ADR không cần dài, nhưng phải đủ lý do.
- Khi quyết định thay đổi, tạo ADR mới thay vì sửa lịch sử tùy tiện.

## Thứ tự đọc

1. `01-adr-use-go-for-data-plane.md`
2. `02-adr-use-aspnetcore-for-control-plane.md`
3. `03-adr-use-kafka-between-ingestion-and-processing.md`
4. `04-adr-use-opensearch-for-log-search.md`

## Kết quả mong đợi sau khi đọc

Sau khi đọc xong nhóm này, người đọc phải hiểu vì sao TraceFlow được chia như hiện tại và những lựa chọn kỹ thuật nào không nên thay đổi tùy tiện nếu chưa có ADR mới.
