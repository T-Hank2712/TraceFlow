# TraceFlow Performance Test 002 Summary

## Context

`test-002` ghi nhận kết quả benchmark của Ingestion API sau các cải tiến về độ tin cậy, bảo vệ request và xử lý payload so với `test-001`.

## Main Changes Since Test 001

| Area | Change |
| ---- | ------ |
| Ingestion reliability | Kafka publish chờ delivery result trước khi trả response, tránh đánh dấu request thành công khi message chưa được broker xác nhận. |
| Authentication path | API key validation được tối ưu bằng cache và single-flight để giảm gọi lặp lại Control API khi có nhiều request cùng API key. |
| Cache safety | Cache key không còn dùng raw API key trực tiếp, tách rõ local cache và Redis cache prefix. |
| Rate limiting | Bổ sung pre-auth rate limit và Redis-backed rate limit để bảo vệ hệ thống khi request thiếu/sai API key hoặc traffic tăng cao. |
| Payload protection | Bổ sung kiểm soát timestamp, trace/correlation id length, metadata size/depth/key limits và xử lý malformed JSON có kiểm soát. |
| Dependency readiness | Bổ sung readiness check cho Redis, Kafka và Control API để phát hiện trạng thái phụ thuộc trước khi nhận tải. |

## Result Notes

* `load`, `spike`, `stress` trong `test-002` đều đạt threshold **error rate < 1%** và **p95 latency < 500 ms**.
* Spike test trong `test-002` là kết quả hợp lệ hơn `test-001` vì không còn tình trạng **100% request thất bại**.
* `load` và `stress` trong `test-002` có throughput thấp hơn `test-001`, nhưng latency đuôi ổn định hơn.
* Capacity benchmark được ghi riêng trong file `capacity-2026-10-13_11-58-00.md`.

## Resource Configuration

| Service       | CPU limit | Memory limit |
| ------------- | --------: | -----------: |
| postgres      |      0.50 |         512M |
| kafka         |      1.00 |        1536M |
| kafka-init    | not configured | not configured |
| opensearch    |      1.50 |        2560M |
| redis         |      1.00 |         128M |
| control-api   |      0.50 |         256M |
| ingestion-api |      0.50 |         256M |
| log-processor |      0.50 |         512M |
| nginx         |      0.10 |          64M |

## Interpretation

So với `test-001`, `test-002` ưu tiên độ tin cậy và tính đúng của ingestion path hơn việc chỉ trả response nhanh. Vì vậy throughput ở một số kịch bản VU cố định có thể thấp hơn, nhưng error handling, Kafka delivery guarantee và tail latency ổn định hơn.
