# Troubleshooting

Tài liệu này ghi các tình huống lỗi thường gặp và cách điều tra.

## Kafka lag tăng

Kiểm tra processor throughput, OpenSearch latency, batch size và số consumer instance.

## Không tìm thấy log

Kiểm tra API key, ingestion response, Kafka topic, processor log, OpenSearch index và tenant filter trong Search API.

## OpenSearch indexing chậm

Kiểm tra bulk size, refresh interval, mapping, shard count, CPU/memory và disk pressure.

## User bị forbidden

Kiểm tra workspace membership, project ownership và authorization policy.
