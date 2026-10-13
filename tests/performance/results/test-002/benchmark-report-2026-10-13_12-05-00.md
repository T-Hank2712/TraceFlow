# TraceFlow Performance Benchmark Report - Test 002

## 1. Tổng quan

`test-002` đánh giá hiệu năng Ingestion API sau các cải tiến về độ tin cậy, bảo vệ request và xử lý payload. Các benchmark chính sử dụng endpoint:

```text
POST /ingestion/v1/logs
```

Các threshold áp dụng:

| Metric | Threshold |
| ------ | --------- |
| Error rate | < 1% |
| p95 latency | < 500 ms |

## 2. Kết quả tổng hợp

| Scenario | Max VUs / Target RPS | Duration | Total Requests | Throughput | p50 | p90 | p95 | Error Rate | Status |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| Load | 10 VUs | 5 min | 269,499 | 898.31 req/s | 7.70 ms | 9.45 ms | 13.12 ms | 0.00% | PASS |
| Spike | 50 VUs | 5m30s | 174,747 | 529.46 req/s | 9.13 ms | 88.06 ms | 92.52 ms | 0.00% | PASS |
| Stress | 20 VUs | 9 min | 430,971 | 798.09 req/s | 8.48 ms | 65.04 ms | 77.07 ms | 0.00% | PASS |
| Capacity | 1,000 RPS | 2 min | 119,927 | 999.35 req/s | 5.70 ms | 7.83 ms | 16.20 ms | 0.00% | PASS |
| Capacity | 1,200 RPS | 2 min | 143,093 | 1,191.59 req/s | 6.84 ms | 128.23 ms | 179.85 ms | 0.58% | PASS |
| Capacity | 1,300 RPS | 2 min | 155,687 | 1,297.30 req/s | 7.47 ms | 63.70 ms | 85.48 ms | 0.00% | PASS |
| Capacity | 2,000 RPS | 2 min | 239,542 | 1,996.10 req/s | 5.61 ms | 16.89 ms | 34.94 ms | 0.64% | PASS |

## 3. Threshold Evaluation

| Scenario | Error Rate | Error Status | p95 Latency | Latency Status |
| --- | ---: | --- | ---: | --- |
| Load | 0.00% | PASS | 13.12 ms | PASS |
| Spike | 0.00% | PASS | 92.52 ms | PASS |
| Stress | 0.00% | PASS | 77.07 ms | PASS |
| Capacity 1,000 RPS | 0.00% | PASS | 16.20 ms | PASS |
| Capacity 1,200 RPS | 0.58% | PASS | 179.85 ms | PASS |
| Capacity 1,300 RPS | 0.00% | PASS | 85.48 ms | PASS |
| Capacity 2,000 RPS | 0.64% | PASS | 34.94 ms | PASS |

## 4. Nhận xét chính

### 4.1. Tất cả benchmark đều đạt threshold

Toàn bộ các kịch bản trong `test-002` đều đạt:

```text
error rate < 1%
p95 latency < 500 ms
```

Điều này cho thấy Ingestion API vẫn duy trì được tính ổn định HTTP trong các workload đã kiểm thử.

### 4.2. Load và stress có error rate sạch

`load`, `spike` và `stress` đều có:

```text
Error rate = 0.00%
```

Đây là điểm tốt nhất của bộ test lần này. Đặc biệt, spike test trong `test-002` hợp lệ hơn `test-001` vì không còn tình trạng request thất bại hàng loạt.

### 4.3. Tail latency được kiểm soát tốt

p95 latency của các benchmark chính:

```text
Load   : 13.12 ms
Spike  : 92.52 ms
Stress : 77.07 ms
```

Tất cả đều thấp hơn nhiều so với threshold `500 ms`. Load có p95 rất thấp, còn spike/stress có p95 cao hơn do workload biến động mạnh hơn nhưng vẫn nằm trong vùng an toàn.

### 4.4. Capacity cho thấy hệ thống còn dư địa xử lý

Capacity benchmark đạt tới mốc `2,000 RPS` với:

```text
Actual throughput: 1,996.10 req/s
Error rate       : 0.64%
p95 latency      : 34.94 ms
```

Mốc `2,000 RPS` vẫn PASS, nhưng error rate đã xuất hiện rõ hơn so với các mốc thấp hơn. Vì vậy nên xem đây là mốc có tải cao và cần được đọc chung với error rate, dropped iterations và log hệ thống.

## 5. Điểm tốt

* Các benchmark chính đều PASS.
* `load`, `spike`, `stress` đều không có HTTP failure.
* p95 latency của load thấp, chỉ **13,12 ms**.
* Spike test không còn lỗi 100% như kết quả cũ.
* Capacity test xác nhận hệ thống có thể đạt các mốc RPS cao hơn workload VU cố định.

## 6. Điểm cần chú ý

* Throughput của `load` và `stress` thấp hơn `test-001`, dù latency đuôi tốt hơn.
* `spike` có throughput thấp hơn các kịch bản khác vì workload spike tạo áp lực đột ngột và giữ p90/p95 cao hơn.
* Capacity ở `1,200 RPS` có p95 **179,85 ms**, cao hơn các mốc lân cận trong lần chạy này.
* Capacity ở `2,000 RPS` có error rate **0,64%**, vẫn PASS nhưng không còn sạch tuyệt đối.
* Các kết quả này phản ánh performance environment tại thời điểm test, không nên diễn giải trực tiếp thành production guarantee.

## 7. Kết luận

`test-002` cho thấy hệ thống Ingestion API ổn định hơn về mặt HTTP reliability và tail latency trong các benchmark chính. So với `test-001`, điểm nổi bật nhất là spike test đã trở thành kết quả hợp lệ, không còn tình trạng toàn bộ request thất bại.

Trong các kịch bản VU cố định, hệ thống duy trì error rate **0,00%** và p95 latency dưới **100 ms**. Capacity benchmark cho thấy hệ thống có thể đạt tới **2,000 RPS** theo threshold hiện tại, nhưng các mốc tải cao đã bắt đầu có error rate nhẹ nên cần tiếp tục theo dõi khi mở rộng benchmark.
