# TraceFlow Performance Test 004 Summary

## Context

`test-004` ghi nhận lại ba benchmark VU-based hiện tại: `load`, `spike` và `stress`. Các kết quả này đo thời gian Ingestion API nhận log và nhận xác nhận từ Kafka qua endpoint `POST /ingestion/v1/logs`.

## Main Result

| Scenario | Result |
| --- | --- |
| Load | PASS, throughput **1.041,63 req/s**, p95 **9,25 ms**, error rate **0,00%**. |
| Spike | PASS, throughput **1.590,60 req/s**, p95 **11,40 ms**, error rate **0,00%**. |
| Stress | PASS, throughput **1.872,63 req/s**, p95 **9,21 ms**, error rate **0,00%**. |
| Capacity | Các lần chạy 2 phút đạt p95 tốt ở một số mốc cao, nhưng tất cả đều có dropped iterations; mốc cân bằng nhất là **3.500 RPS**. |

## Resource Configuration

| Service | CPU limit | Memory limit |
| --- | ---: | ---: |
| postgres | 0.50 | 512M |
| kafka | 1.50 | 2048M |
| kafka-init | not configured | not configured |
| opensearch | 1.50 | 2560M |
| redis | 1.00 | 256M |
| control-api | 1.00 | 512M |
| ingestion-api | 1.00 | 256M |
| log-processor | 0.50 | 512M |
| nginx | 1.00 | 128M |

## Relevant Runtime Configuration

| Component | Configuration used for test-004 |
| --- | --- |
| Kafka topic | `traceflow.logs`, 3 partitions |
| Ingestion Kafka producer | `acks=all`, `linger.ms=5`, batch publish concurrency `20` |
| Log processor | batch size `100`, flush interval `1000 ms`, queue capacity `10000` per partition lane |
| Log processor retry | maximum `3` retries, backoff `500 ms` |
| Redis | AOF persistence enabled, max memory policy `allkeys-lru` |
| Nginx | 4,096 worker connections, `nofile` limit 16,384 |

Các bảng trên phản ánh compose performance và env dùng khi tổng hợp `test-004`; không ghi secrets hoặc credential vào báo cáo.

## Threshold Evaluation

| Scenario | Error rate < 1% | p95 < 500 ms | p95 < 20 ms |
| --- | --- | --- | --- |
| Load | PASS | PASS | PASS |
| Spike | PASS | PASS | PASS |
| Stress | PASS | PASS | PASS |

## Key Observations

* Cả ba kịch bản có **0,00% HTTP error** và toàn bộ check `status is 200` đều thành công.
* p95 nằm trong khoảng **9,21-11,40 ms**, cho thấy response của ingestion path ổn định với các workload VU-based đã chạy.
* `stress` xử lý hơn **1 triệu request** trong 9 phút mà không có suy giảm p95 rõ rệt.
* `spike` có p95 cao nhất trong ba kịch bản, nhưng vẫn dưới 20 ms. Maximum latency 396,27 ms cho thấy vẫn có một số outlier ngắn khi tăng tải đột ngột.
* Capacity 2 phút không có HTTP error ở 3.500, 4.000, 4.500 và 4.600 RPS, nhưng đều có dropped iterations. `3.500 RPS` chỉ bỏ 648 iterations, thấp nhất trong nhóm.
* Mốc 4.000 RPS không ổn định với p95 173,15 ms, 11.572 dropped iterations và k6 chạm 1.000 VUs. Mốc 4.500 RPS có p95 thấp nhưng vẫn bỏ 2.685 iterations, nên không thể xem là capacity bền vững.
* Các kết quả VU-based không thay thế capacity test theo arrival rate. Chúng không xác nhận hệ thống duy trì được 4.500-5.000 req/s trong 15 phút, vì workload đó đã có tail latency và dropped iterations cao.

## Conclusion

`test-004` PASS cho cả `load`, `spike` và `stress` theo threshold hiện tại, đồng thời cũng đạt tiêu chí p95 nghiêm ngặt hơn là dưới 20 ms. Với capacity arrival-rate, 3.500 RPS là mốc ngắn hạn tốt nhất cần chạy xác minh 15 phút; chưa có bằng chứng để xem 4.000+ RPS là bền vững.
