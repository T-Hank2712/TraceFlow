# TraceFlow Performance Benchmark Comparison

## 1. Tổng quan

Bộ benchmark gồm 4 kịch bản: **Baseline, Load, Stress và Spike**. Các bài test đều sử dụng endpoint `POST /ingestion/v1/logs` trong performance environment và cùng áp dụng hai threshold:

- **HTTP error rate < 1%**
- **p95 latency < 500 ms**

Mục tiêu của phần so sánh này là đánh giá sự thay đổi về throughput, latency và error rate khi tải tăng từ mức cơ sở lên mức stress và spike.

## 2. So sánh tổng quan

| Scenario | Max VUs | Duration | Total Requests | Throughput | p50 | p90 | p95 | Error Rate |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Baseline | 5 | 1 min | 70,968 | 1,182.78 req/s | 0.721 ms | 1.29 ms | 2.56 ms | 0.00% |
| Load | 10 | 5 min | 362,773 | 1,209.27 req/s | 1.01 ms | 2.28 ms | 82.24 ms | 0.00% |
| Stress | 20 | 9 min | 668,662 | 1,238.26 req/s | 1.66 ms | 82.50 ms | 87.50 ms | 0.00% |
| Spike | 50 | 5 min | 382,813 | 1,159.94 req/s | 1.43 ms | 86.43 ms | 91.19 ms | 0.00% |

### Threshold

| Metric | Threshold | Baseline | Load | Stress | Spike |
|---|---:|---:|---:|---:|---:|
| Error rate | < 1% | PASS | PASS | PASS | PASS |
| p95 latency | < 500 ms | PASS | PASS | PASS | PASS |

## 3. Phân tích Throughput

| Scenario | Throughput |
|---|---:|
| Baseline - 5 VUs | 1,182.78 req/s |
| Load - 10 VUs | 1,209.27 req/s |
| Stress - 20 VUs | 1,238.26 req/s |
| Spike - 50 VUs | 1,159.94 req/s |

- Từ **5 → 10 VUs**, throughput tăng khoảng **2.24%**.
- Từ **10 → 20 VUs**, throughput tăng khoảng **2.40%**.
- Từ **20 → 50 VUs**, throughput giảm khoảng **6.33%**.
- So với baseline, stress test đạt throughput cao hơn khoảng **4.69%**.

Số VUs tăng 4 lần từ 5 lên 20 nhưng throughput chỉ tăng khoảng 4.69%. Khi tăng tiếp lên 50 VUs, throughput giảm xuống **1,159.94 req/s**.

Điều này cho thấy trong environment hiện tại, hệ thống có dấu hiệu đạt **throughput plateau / saturation** quanh mức khoảng **1.2k req/s**.

Tuy nhiên, kết quả này **chưa đủ để xác định component nào là bottleneck**. Cần kết hợp CPU, memory, Kafka lag, OpenSearch và các resource metrics khác.

## 4. Phân tích Latency

### p50

| Scenario | p50 |
|---|---:|
| Baseline | 0.721 ms |
| Load | 1.01 ms |
| Stress | 1.66 ms |
| Spike | 1.43 ms |

p50 tăng từ **0.721 ms** ở baseline lên **1.66 ms** ở stress test nhưng vẫn ở mức thấp.

### p90

| Scenario | p90 |
|---|---:|
| Baseline | 1.29 ms |
| Load | 2.28 ms |
| Stress | 82.50 ms |
| Spike | 86.43 ms |

p90 tăng mạnh từ Load sang Stress, từ **2.28 ms lên 82.50 ms**. Spike tiếp tục ở mức **86.43 ms**.

Điều này cho thấy khi workload tăng, một phần request bắt đầu có response time cao hơn mặc dù median latency vẫn thấp.

### p95

| Scenario | p95 |
|---|---:|
| Baseline | 2.56 ms |
| Load | 82.24 ms |
| Stress | 87.50 ms |
| Spike | 91.19 ms |

So với baseline:

- Load: p95 tăng từ **2.56 ms → 82.24 ms**, khoảng **32.1 lần**.
- Stress: p95 khoảng **34.2 lần** baseline.
- Spike: p95 khoảng **35.6 lần** baseline.

Mặc dù p95 tăng mạnh, tất cả benchmark vẫn nằm dưới threshold **500 ms**.

Đây là dấu hiệu rõ ràng của **tail latency degradation** khi concurrency tăng.

## 5. Phân tích Error Rate

Error rate của cả 4 benchmark đều bằng **0.00%**.

| Scenario | Error Rate | Result |
|---|---:|---|
| Baseline | 0.00% | PASS |
| Load | 0.00% | PASS |
| Stress | 0.00% | PASS |
| Spike | 0.00% | PASS |

Ngay cả spike test với tối đa **50 VUs** cũng không ghi nhận HTTP request failure trong kết quả được cung cấp.

Tuy nhiên, HTTP error rate chỉ phản ánh tầng HTTP. Kết quả này chưa tự động chứng minh toàn bộ pipeline phía sau luôn xử lý không có lỗi. E2E delivery và Kafka/OpenSearch metrics cần được kiểm tra riêng.

## 6. Những điểm tốt

### 6.1. HTTP reliability tốt trong các workload đã kiểm thử

Cả 4 benchmark đều đạt:

- Error rate = **0.00%**
- p95 < **500 ms**

### 6.2. Throughput duy trì quanh mức 1.2k req/s

Throughput dao động trong khoảng:

**1,159.94 → 1,238.26 req/s**

Ngay cả khi workload tăng từ 5 lên 50 VUs, throughput vẫn nằm trong cùng một order of magnitude.

### 6.3. Median latency thấp

p50 của cả 4 benchmark nằm trong khoảng:

**0.721 → 1.66 ms**

### 6.4. Tail latency vẫn dưới threshold

p95 cao nhất là **91.19 ms**, vẫn thấp hơn đáng kể so với threshold **500 ms**.

## 7. Các vấn đề đang gặp

### 7.1. Throughput có dấu hiệu saturation

Đây là vấn đề nổi bật nhất:

```text
5 VUs  → 1,182.78 req/s
10 VUs → 1,209.27 req/s
20 VUs → 1,238.26 req/s
50 VUs → 1,159.94 req/s
```

Tăng workload không tạo ra throughput tăng tương ứng. Ở 50 VUs, throughput còn giảm.

Điều này cho thấy hệ thống có thể đang tiến gần giới hạn của một hoặc nhiều thành phần trong performance environment.

**Chưa thể kết luận component cụ thể nào là bottleneck chỉ từ kết quả k6.**

### 7.2. Tail latency tăng mạnh

p95 tăng:

```text
Baseline → 2.56 ms
Load     → 82.24 ms
Stress   → 87.50 ms
Spike    → 91.19 ms
```

Sự thay đổi lớn nhất xảy ra khi chuyển từ baseline sang load.

### 7.3. Chưa có resource metrics đi kèm

Các benchmark hiện tại có request count, throughput, p50, p90, p95 và error rate nhưng chưa có dữ liệu đồng bộ về:

- CPU utilization
- Memory utilization
- Kafka consumer lag
- Kafka throughput
- OpenSearch indexing performance
- PostgreSQL utilization
- Redis utilization
- Network utilization

Do đó chưa thể xác định chính xác nguyên nhân của throughput plateau và tail latency increase.

### 7.4. Kết quả phụ thuộc vào performance environment

Các con số phản ánh **performance environment hiện tại**, không phải production capacity guarantee.

Không nên diễn giải thành:

> "TraceFlow hỗ trợ 1,238 req/s."

Cách diễn đạt chính xác hơn:

> "TraceFlow đạt throughput đo được khoảng 1,238 req/s trong stress benchmark dưới performance environment hiện tại."

## 8. Nhược điểm của bộ benchmark hiện tại

### 8.1. Chưa xác định bottleneck

Benchmark đã phát hiện dấu hiệu saturation nhưng chưa chỉ ra nguyên nhân.

Cần correlation giữa workload và resource utilization của:

```text
Nginx
  ↓
Ingestion API
  ↓
Kafka
  ↓
Log Processor
  ↓
OpenSearch
```

### 8.2. Chưa kiểm tra network impairment

Chưa có kết quả cho:

- Network latency
- Jitter
- Packet loss
- Bandwidth limitation
- Recovery sau network degradation

### 8.3. Chưa đánh giá long-running stability

Các benchmark hiện tại kéo dài từ 1 đến 9 phút. Chưa có soak/endurance test đủ dài để quan sát:

- memory growth
- Kafka lag accumulation
- resource exhaustion
- latency degradation theo thời gian
- stability của OpenSearch indexing

### 8.4. Chưa xác định capacity limit

Chưa xác định:

- maximum sustainable throughput
- workload tại đó error rate bắt đầu tăng
- workload tại đó p95/p99 vượt threshold
- thời điểm Kafka lag bắt đầu tăng liên tục

Vì vậy hiện tại chỉ nên gọi đây là **observed performance range**, chưa phải capacity limit chính thức.

## 9. Tổng hợp đánh giá

| Aspect | Kết quả quan sát |
|---|---|
| HTTP reliability | 0.00% error rate trong các workload đã test |
| Median latency | Thấp, khoảng 0.7–1.7 ms |
| Tail latency | Tăng mạnh khi workload tăng |
| Throughput | Duy trì quanh ~1.2k req/s |
| Scalability theo VUs | Có dấu hiệu không tuyến tính |
| Saturation | Có dấu hiệu xuất hiện |
| Bottleneck identification | Chưa đủ dữ liệu |
| Network resilience | Chưa đánh giá |
| Long-running stability | Chưa đánh giá |
| Capacity limit | Chưa xác định chính thức |

## 10. Kết luận

Bộ benchmark cho thấy TraceFlow có **HTTP success rate 100%** và p95 latency vẫn nằm dưới threshold 500 ms trong cả Baseline, Load, Stress và Spike test.

Điểm đáng chú ý nhất là mối quan hệ giữa **workload, throughput và tail latency**:

```text
5 VUs  → 1,182.78 req/s → p95 2.56 ms
10 VUs → 1,209.27 req/s → p95 82.24 ms
20 VUs → 1,238.26 req/s → p95 87.50 ms
50 VUs → 1,159.94 req/s → p95 91.19 ms
```

Kết quả cho thấy throughput có xu hướng **plateau quanh ~1.2k req/s**, trong khi tail latency tăng đáng kể khi concurrency tăng.

Đây là tín hiệu performance quan trọng cần điều tra thêm, nhưng **chưa đủ dữ liệu để kết luận component bottleneck**.

### Các bước tiếp theo

1. Thu thập CPU và memory theo từng container trong lúc benchmark.
2. Theo dõi Kafka consumer lag trong Load/Stress/Spike.
3. Correlate resource utilization với thời điểm p90/p95 tăng.
4. Thực hiện network impairment test với latency, jitter, packet loss và bandwidth limitation.
5. Bổ sung soak/endurance test để kiểm tra stability dài hạn.
6. Tiếp tục tăng workload có kiểm soát để xác định vùng saturation và capacity boundary.
7. Ghi nhận benchmark kết hợp với resource metrics để tạo baseline có thể tái lập.

> **Lưu ý:** Các kết luận trên chỉ dựa trên 4 bộ kết quả benchmark được cung cấp. Chưa sử dụng CPU, memory, Kafka lag hoặc OpenSearch metrics để xác định nguyên nhân bottleneck.
