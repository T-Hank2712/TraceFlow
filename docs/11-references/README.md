# References

Nhóm tài liệu này chứa ghi chú tham khảo và kiến thức nền khi thiết kế TraceFlow. Đây không phải contract chính thức như `api/`, `data/` hoặc `architecture/`, nhưng là nơi lưu kiến thức giúp hiểu các quyết định kỹ thuật.

References nên được dùng để học, giải thích và ghi chú benchmark. Nếu một ghi chú trong folder này trở thành quyết định bắt buộc của hệ thống, nội dung đó nên được chuyển hoặc liên kết sang architecture/domain/data docs tương ứng.

## Nội dung cần nắm

- Glossary thống nhất thuật ngữ.
- System design notes ghi các câu hỏi thiết kế quan trọng.
- Kafka notes tóm tắt topic, partition, consumer group, offset và lag.
- OpenSearch notes tóm tắt indexing, mapping, query, aggregation và rủi ro.
- Performance benchmark notes ghi cách đo và diễn giải số liệu.

## Khi nào cần cập nhật nhóm này

- Khi học được khái niệm mới liên quan trực tiếp tới TraceFlow.
- Khi benchmark có kết quả mới.
- Khi cần ghi lại câu hỏi system design để thảo luận.
- Khi thuật ngữ mới xuất hiện trong docs hoặc code.

## Thứ tự đọc

1. `01-glossary.md`
2. `02-system-design-notes.md`
3. `03-kafka-notes.md`
4. `04-opensearch-notes.md`
5. `05-performance-benchmark-notes.md`

## Kết quả mong đợi sau khi đọc

Sau khi đọc xong nhóm này, người đọc có nền tảng thuật ngữ và khái niệm đủ để hiểu các tài liệu kiến trúc, dữ liệu và testing sâu hơn.
