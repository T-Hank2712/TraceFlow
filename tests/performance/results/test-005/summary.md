# Tóm tắt Hiệu năng TraceFlow - Test 005

## Phạm vi

`test-005` tổng hợp:

* Năm lượt capacity 2 phút từ `4.500` đến `5.300 RPS`.
* Ba lượt capacity duy trì 15 phút tại `4.000`, `4.250` và `4.500 RPS`.
* Một lượt đối soát E2E độc lập với 50.000 event.

## Kết quả chính

| Hạng mục | Kết quả |
| --- | --- |
| Capacity 2 phút | `5.000 RPS` là mốc PASS cao nhất: **4.996,29 req/s**, p95 **7,94 ms**, HTTP failure **0,00%**. |
| Giới hạn 2 phút | `5.200 RPS` fail p95 tại **10,88 ms**; `5.300 RPS` quá tải với p95 **142,18 ms** và **32.866** dropped iterations. |
| Capacity 15 phút | `4.250 RPS` là cận trên PASS quan sát được: **4.217,43 req/s**, p95 **8,59 ms**, nhưng có **29.270** dropped iterations. |
| Mốc 15 phút thận trọng | `4.000 RPS`: **3.981,38 req/s**, p95 **8,78 ms**, **16.711** dropped iterations. |
| Đối soát E2E | **50.000 / 50.000** event được index; không có event thiếu, DLQ, không mong đợi hoặc trùng. |

## Nhận xét chính

* Mọi lượt capacity trong bộ kết quả có `0,00%` HTTP failure; latency và dropped iterations là yếu tố phân biệt capacity.
* `5.000 RPS` phù hợp để mô tả burst capacity ngắn hạn 2 phút theo threshold hiện tại.
* `4.500 RPS` không đạt hiệu năng bền vững trong 15 phút: p95 tăng đến **189,64 ms** và k6 drop **142.936** iteration.
* Dù `4.250 RPS` PASS latency 15 phút, việc chạm 2.000 VU và có dropped iterations cho thấy mốc này nên được xem là cận trên, không phải capacity không suy giảm.
* E2E xác nhận tính đúng đắn downstream: event được nhận, chuyển qua pipeline và index đúng một lần cho workload đã chạy.

## Kết luận

* Công bố thận trọng cho tải duy trì 15 phút: **4.000 RPS**.
* Cận trên PASS 15 phút đã quan sát: **4.250 RPS**.
* Công bố cho burst capacity 2 phút: **5.000 RPS**.
* Không công bố `4.500 RPS` là sustained capacity hoặc `5.200 RPS` là short-duration capacity đạt mục tiêu p95.
