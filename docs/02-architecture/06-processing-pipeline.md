# Processing Pipeline

Processing pipeline đọc log từ Kafka, xử lý và index vào OpenSearch.

## Các bước xử lý

```text
Consume -> Decode -> Validate internal event -> Enrich -> Batch -> Bulk Index -> Commit Offset
```

## Batch processing

Processor gom log theo batch size hoặc flush interval. Mục tiêu là giảm số network call tới OpenSearch và tăng throughput.

## Offset management

Offset chỉ nên commit sau khi batch đã được xử lý thành công theo policy đã định. Nếu commit quá sớm, hệ thống có thể mất log khi processor crash.

## Failure policy

Processor retry có giới hạn với exponential backoff. Sau khi vượt ngưỡng retry, event lỗi được đưa vào DLQ kèm lý do lỗi, thời điểm lỗi và thông tin batch để có thể điều tra hoặc reprocess.
