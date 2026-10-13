# TraceFlow Performance Benchmark Report - Test 003

## 1. Tổng quan

`test-003` tổng hợp kết quả benchmark của Ingestion API trong môi trường performance sau khi cấp thêm resource cho các service trọng yếu.

Endpoint được kiểm thử:

```text
POST /ingestion/v1/logs
```

Threshold áp dụng:

| Metric | Threshold |
| ------ | --------- |
| Error rate | < 1% |
| p95 latency | < 500 ms |

## 2. Kết quả tổng hợp

| Scenario | Max VUs / Target RPS | Duration | Total Requests | Throughput | p50 | p90 | p95 | Error Rate | Status |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| Load | 10 VUs | 5 min | 298,424 | 994.74 req/s | 7.91 ms | 9.28 ms | 9.84 ms | 0.00% | PASS |
| Spike | 50 VUs | 5m30s | 660,914 | 2,002.73 req/s | 7.93 ms | 9.39 ms | 9.99 ms | 0.00% | PASS |
| Stress | 20 VUs | 9 min | 960,669 | 1,778.99 req/s | 7.89 ms | 9.24 ms | 9.77 ms | 0.00% | PASS |
| Capacity | 2,200 RPS | 2 min | 263,292 | 2,193.86 req/s | 5.57 ms | 19.86 ms | 40.36 ms | 0.33% | PASS |
| Capacity | 2,500 RPS | 2 min | 299,586 | 2,496.42 req/s | 5.10 ms | 8.09 ms | 16.79 ms | 0.00% | PASS |
| Capacity | 2,700 RPS | 2 min | 323,110 | 2,692.49 req/s | 5.25 ms | 9.21 ms | 21.30 ms | 0.01% | PASS |
| Capacity | 3,000 RPS | 2 min | 354,608 | 2,954.90 req/s | 4.91 ms | 7.44 ms | 8.13 ms | 0.91% | PASS |

## 3. Threshold Evaluation

| Scenario | Error Rate | Error Status | p95 Latency | Latency Status |
| --- | ---: | --- | ---: | --- |
| Load | 0.00% | PASS | 9.84 ms | PASS |
| Spike | 0.00% | PASS | 9.99 ms | PASS |
| Stress | 0.00% | PASS | 9.77 ms | PASS |
| Capacity 2,200 RPS | 0.33% | PASS | 40.36 ms | PASS |
| Capacity 2,500 RPS | 0.00% | PASS | 16.79 ms | PASS |
| Capacity 2,700 RPS | 0.01% | PASS | 21.30 ms | PASS |
| Capacity 3,000 RPS | 0.91% | PASS | 8.13 ms | PASS |

## 4. Nhận xét chính

### 4.1. Load, spike và stress đều sạch lỗi

Ba kịch bản VU cố định đều đạt:

```text
Error rate = 0.00%
```

Đây là kết quả tốt nhất của `test-003`, cho thấy hệ thống giữ ổn định HTTP trong các workload chính.

### 4.2. Tail latency rất thấp

p95 latency của các benchmark chính:

```text
Load   : 9.84 ms
Spike  : 9.99 ms
Stress : 9.77 ms
```

Các giá trị này thấp hơn rất nhiều so với threshold **500 ms**, đồng thời ổn định hơn so với các bộ test trước.

### 4.3. Spike và stress cải thiện rõ rệt

`spike` đạt **2.002,73 req/s** và `stress` đạt **1.778,99 req/s**, đều không có HTTP failure. Đây là cải thiện lớn so với kết quả `test-002`.

### 4.4. Capacity mở rộng tốt tới vùng 2.700 RPS

Capacity benchmark cho thấy:

```text
2,500 RPS: 0.00% error, p95 16.79 ms
2,700 RPS: 0.01% error, p95 21.30 ms
```

Hai mốc này là vùng tốt nhất để mô tả khả năng chịu tải cao trong bộ kết quả hiện tại.

### 4.5. 3.000 RPS là vùng sát giới hạn

Mốc **3.000 RPS** vẫn PASS nhưng có:

```text
Error rate          : 0.91%
Dropped iterations  : 5,391
```

Do đó, 3.000 RPS nên được xem là **edge passing capacity**, không phải mốc stable clean.

## 5. Điểm tốt

* Tất cả benchmark đều PASS threshold.
* `load`, `spike`, `stress` đều có error rate **0,00%**.
* p95 latency của các kịch bản chính đều quanh **10 ms**.
* `spike` đạt hơn **2.000 req/s** mà vẫn không có HTTP failure.
* `stress` xử lý hơn **960.000 request** trong 9 phút với error rate **0,00%**.
* Capacity đạt sạch ở **2.500 RPS** và gần sạch ở **2.700 RPS**.

## 6. Điểm cần chú ý

* Mốc **3.000 RPS** đã sát ngưỡng lỗi, với error rate **0,91%**.
* Dropped iterations ở **3.000 RPS** tăng lên **5.391**, cho thấy k6/hệ thống đã gần vùng giới hạn.
* Các kết quả này phản ánh performance environment tại thời điểm test, không nên diễn giải trực tiếp thành production guarantee.

## 7. Kết luận

`test-003` cho thấy Ingestion API đạt kết quả tốt nhất trong các bộ benchmark hiện tại. Hệ thống xử lý ổn định ở các kịch bản `load`, `spike`, `stress`, đồng thời mở rộng tốt ở capacity benchmark.

Mốc **2.500 RPS** có thể xem là **stable clean capacity** trong bộ test này. Mốc **2.700 RPS** là **upper passing capacity** tốt. Mốc **3.000 RPS** vẫn PASS nhưng đã sát ngưỡng lỗi, nên chỉ nên xem là **edge passing capacity**.
