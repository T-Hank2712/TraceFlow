# Báo cáo Benchmark Hiệu năng TraceFlow - Test 005

## 1. Tổng quan

`test-005` ghi nhận kết quả capacity arrival-rate hiện tại của Ingestion API, bao gồm nhóm chạy ngắn 2 phút, nhóm chạy duy trì 15 phút và một lượt đối soát end-to-end độc lập.

Endpoint capacity được kiểm thử:

```text
POST /ingestion/v1/logs
```

## 2. Capacity 2 phút

| RPS mục tiêu | RPS thực tế | p95 | HTTP thất bại | Dropped iterations | Trạng thái |
| ---: | ---: | ---: | ---: | ---: | --- |
| 4.500 | 4.481,31 | 8,09 ms | 0,00% | 2.226 | PASS |
| 4.700 | 4.690,47 | 8,09 ms | 0,00% | 1.107 | PASS |
| 5.000 | 4.996,29 | 7,94 ms | 0,00% | 411 | PASS |
| 5.200 | 5.172,84 | 10,88 ms | 0,00% | 3.213 | FAIL |
| 5.300 | 5.025,82 | 142,18 ms | 0,00% | 32.866 | FAIL |

Ngưỡng capacity 2 phút là p95 dưới **10 ms** và tỷ lệ HTTP thất bại dưới **0,01%**.

`5.000 RPS` là target ngắn hạn cao nhất đạt cả hai ngưỡng. Từ `5.200 RPS`, p95 đã vượt ngưỡng; tại `5.300 RPS`, throughput không còn theo kịp target và k6 chạm giới hạn 1.000 VU.

## 3. Capacity duy trì 15 phút

| RPS mục tiêu | RPS thực tế | p95 | HTTP thất bại | Dropped iterations | Trạng thái |
| ---: | ---: | ---: | ---: | ---: | --- |
| 4.000 | 3.981,38 | 8,78 ms | 0,00% | 16.711 | PASS |
| 4.250 | 4.217,43 | 8,59 ms | 0,00% | 29.270 | PASS |
| 4.500 | 4.340,57 | 189,64 ms | 0,00% | 142.936 | FAIL |

`4.250 RPS` là mốc 15 phút cao nhất PASS theo ngưỡng latency trong tập kết quả. Tuy nhiên, cả 4.000 và 4.250 RPS đều có dropped iterations và chạm giới hạn VU, nên `4.000 RPS` là mốc thận trọng hơn nếu cần công bố sustained capacity có biên an toàn.

## 4. Soak test 60 phút

| RPS mục tiêu | RPS thực tế | Thời lượng | p95 | HTTP failure | Dropped iterations | Trạng thái soak |
| ---: | ---: | ---: | ---: | ---: | ---: | --- |
| 3.000 | 2.957,30 | 60 phút | 93,12 ms | xấp xỉ 0,000009% | 153.694 | PASS |

Soak test dùng tiêu chí p95 dưới **100 ms** và HTTP failure dưới **0,01%**. Do đó p95 93,12 ms là PASS theo tiêu chí soak, mặc dù output k6 vẫn hiển thị threshold cũ `p(95)<10` là FAIL. Lượt chạy hoàn tất đủ 60 phút, nhưng k6 chạm 3.000 VU và có dropped iterations; vì vậy throughput thực tế là 2.957,30 req/s thay vì target danh nghĩa 3.000 RPS.

## 5. Đối soát E2E

| Chỉ số | Kết quả |
| --- | ---: |
| Event sinh ra | 50.000 |
| Event đã index | 50.000 |
| Event bị thiếu | 0 |
| Event vào DLQ | 0 |
| Khóa event trùng trong OpenSearch | 0 |
| Độ trễ HTTP p95 | 9,98 ms |
| Kết quả E2E | PASS |

Lượt đối soát xác nhận toàn bộ 50.000 event được index đúng một lần cho workload E2E đã kiểm thử.

## 6. Kết luận

* Với capacity ngắn 2 phút, **5.000 RPS** là mốc PASS tốt nhất.
* Với capacity duy trì 15 phút, **4.250 RPS** là cận trên PASS quan sát được; **4.000 RPS** là lựa chọn thận trọng hơn do có biên tốt hơn trước trạng thái quá tải.
* `4.500 RPS` không bền vững trong 15 phút và `5.200 RPS` không đạt p95 trong lượt 2 phút.
* Soak test **3.000 RPS trong 60 phút** PASS theo tiêu chí p95 dưới 100 ms và HTTP failure dưới 0,01%; throughput thực tế là 2.957,30 req/s.
* E2E PASS với 50.000 event được index đầy đủ, không có DLQ, thiếu hoặc trùng event.
