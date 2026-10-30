# TraceFlow Performance Benchmark Report - Test 004

## 1. Tổng quan

`test-004` tổng hợp benchmark của Ingestion API trong môi trường performance: `load`, `spike`, `stress` và các mốc capacity 2 phút từ 3.000 đến 4.600 RPS.

Endpoint được kiểm thử:

```text
POST /ingestion/v1/logs
```

Threshold được cấu hình trong cả ba kịch bản:

| Metric | Threshold |
| --- | --- |
| Error rate | < 1% |
| p95 latency | < 500 ms |

## 2. Môi trường benchmark

| Service | CPU limit | Memory limit |
| --- | ---: | ---: |
| postgres | 0.50 | 512M |
| kafka | 1.50 | 2048M |
| opensearch | 1.50 | 2560M |
| redis | 1.00 | 256M |
| control-api | 1.00 | 512M |
| ingestion-api | 1.00 | 256M |
| log-processor | 0.50 | 512M |
| nginx | 1.00 | 128M |

Kafka dùng topic 3 partitions. Log processor flush batch 100 events mỗi 1.000 ms với queue capacity 10.000 cho mỗi partition lane. Redis bật AOF; Nginx cấu hình 4.096 worker connections và `nofile` 16.384.

## 3. Kết quả tổng hợp

| Scenario | Max VUs | Duration | Total Requests | Throughput | p50 | p90 | p95 | Error Rate | Status |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| Load | 10 | 5 min | 312,496 | 1,041.63 req/s | 7.46 ms | 8.71 ms | 9.25 ms | 0.00% | PASS |
| Spike | 50 | 5m30s | 524,906 | 1,590.60 req/s | 7.75 ms | 9.66 ms | 11.40 ms | 0.00% | PASS |
| Stress | 20 | 9 min | 1,011,239 | 1,872.63 req/s | 7.43 ms | 8.68 ms | 9.21 ms | 0.00% | PASS |
| Capacity | 3,000 RPS | 2 min | 358,379 | 2,986.40 req/s | 4.82 ms | 7.32 ms | 7.85 ms | 0.00% | PASS |
| Capacity | 3,500 RPS | 2 min | 419,353 | 3,494.41 req/s | 4.89 ms | 7.38 ms | 7.97 ms | 0.00% | PASS |
| Capacity | 4,000 RPS | 2 min | 468,428 | 3,902.35 req/s | 6.95 ms | 109.27 ms | 173.15 ms | 0.00% | PASS |
| Capacity | 4,500 RPS | 2 min | 537,316 | 4,477.45 req/s | 4.96 ms | 7.37 ms | 8.00 ms | 0.00% | PASS |
| Capacity | 4,600 RPS | 2 min | 540,488 | 4,503.74 req/s | 5.20 ms | 7.90 ms | 9.68 ms | 0.00% | PASS |

## 4. Nhận xét chính

### 4.1. Response HTTP ổn định trong ba benchmark

Toàn bộ 1.848.641 request của ba kịch bản trả HTTP 200. Không có check hoặc HTTP request nào thất bại.

### 4.2. Tail latency thấp hơn ngưỡng nghiêm ngặt

Mặc dù k6 chỉ đặt threshold p95 dưới 500 ms, cả ba kết quả đều dưới 20 ms:

```text
Load   : 9.25 ms
Spike  : 11.40 ms
Stress : 9.21 ms
```

### 4.3. Spike có outlier nhưng không làm xấu p95

Kịch bản spike có maximum latency 396,27 ms, cao hơn hai kịch bản còn lại. Tuy vậy, p95 vẫn 11,40 ms và error rate bằng 0, cho thấy đây là outlier ngắn thay vì suy giảm diện rộng.

### 4.4. Capacity cao chưa đạt yêu cầu bền vững

Các mốc capacity đều pass threshold HTTP đã cấu hình, nhưng tất cả đều có dropped iterations. Điều này nghĩa là k6 không phát đủ lượng iteration target trong hai phút.

```text
3,500 RPS : 648 dropped iterations, p95 7.97 ms
4,000 RPS : 11,572 dropped iterations, p95 173.15 ms, 1,000 VUs
4,500 RPS : 2,685 dropped iterations, p95 8.00 ms
4,600 RPS : 11,514 dropped iterations, max latency 2.02 s
```

`3.500 RPS` là mốc tốt nhất trong bộ kết quả mới, nhưng vẫn cần chạy 15 phút trên môi trường đã làm sạch trước khi được xem là sustained capacity.

## 5. Giới hạn diễn giải

Các kịch bản VU-based dùng VUs theo stage, nên throughput là kết quả hệ thống tự đạt được dưới mức đồng thời tương ứng, không phải tải arrival-rate bắt buộc. Capacity 2 phút cũng chỉ là kết quả ngắn hạn. Không dùng bộ kết quả này để kết luận capacity bền vững ở 4.500 hoặc 5.000 req/s. Capacity test 15 phút cần được đánh giá bằng p95, throughput thực tế và dropped iterations riêng.

## 6. Kết luận

`test-004` xác nhận Ingestion API đáp ứng tốt ở workload `load`, `spike` và `stress` hiện tại: không có lỗi HTTP, p95 dưới 12 ms và stress xử lý hơn một triệu request trong 9 phút. Ở capacity, 3.500 RPS là mốc ngắn hạn hứa hẹn nhất; 4.000-4.600 RPS chưa đủ ổn định để công bố là sustained capacity.
