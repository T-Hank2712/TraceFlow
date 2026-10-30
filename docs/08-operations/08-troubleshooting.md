# Troubleshooting

Tài liệu này ghi các tình huống lỗi thường gặp và cách điều tra.

## Kafka lag tăng

Kiểm tra processor throughput, OpenSearch latency, batch size và số consumer instance.

## Không tìm thấy log

Kiểm tra API key, ingestion response, Kafka topic, processor log, OpenSearch index và tenant filter trong Search API.

## OpenSearch indexing chậm

Kiểm tra bulk size, refresh interval, mapping, shard count, CPU/memory và disk pressure.

## Log Processor crash khi khởi động vì mapping

Processor chỉ chạy với index có `dynamic: false` và `metadata: flat_object`. Kiểm tra mapping của `OpenSearch__Index`. Không sửa type field của index đã tồn tại tại chỗ; tạo index mới hoặc reindex rồi cập nhật `OpenSearch__Index`. Với performance stack không cần giữ dữ liệu, xóa index cũ rồi restart Log Processor để processor tạo mapping mới.

## E2E verifier không tìm thấy workload metadata

Kiểm tra Log Processor có đang chạy và Kafka consumer lag. Nếu processor từng crash hoặc Kafka có backlog từ benchmark trước, đợi lag về 0 hoặc làm sạch Kafka/OpenSearch trước khi chạy E2E. Rebuild `traceflow-e2e-verifier` sau khi thay đổi source verifier.

## User bị forbidden

Kiểm tra workspace membership, project ownership và authorization policy.
