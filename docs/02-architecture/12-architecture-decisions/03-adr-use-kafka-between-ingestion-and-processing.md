# ADR-0003: Dùng Kafka Giữa Ingestion Và Processing

## Trạng thái

Chấp nhận.

## Bối cảnh

Ingestion API cần trả response nhanh, trong khi OpenSearch indexing có thể chậm, timeout hoặc cần batch.

## Quyết định

Đặt Kafka giữa Go Ingestion Service và Go Log Processor.

## Lý do

- Tách ingestion khỏi indexing.
- Hấp thụ traffic spike.
- Cho phép scale consumer độc lập.
- Theo dõi được consumer lag.
- Hỗ trợ retry và DLQ.

## Hệ quả

Hệ thống có thêm độ phức tạp vận hành. Cần định nghĩa topic, partition, consumer group, offset management và failure policy rõ ràng.
