# TraceFlow Performance Test 003 Summary

## Context

`test-003` ghi nhận kết quả benchmark sau khi môi trường performance được cấp thêm resource cho các service trọng yếu. Mục tiêu của bộ test này là đánh giá lại khả năng xử lý của Ingestion API trong cùng nhóm kịch bản `load`, `spike`, `stress` và `capacity`.

## Main Result

| Scenario | Result |
| -------- | ------ |
| Load | PASS, throughput đạt **994,74 req/s**, p95 **9,84 ms**, error rate **0,00%**. |
| Spike | PASS, throughput đạt **2.002,73 req/s**, p95 **9,99 ms**, error rate **0,00%**. |
| Stress | PASS, throughput đạt **1.778,99 req/s**, p95 **9,77 ms**, error rate **0,00%**. |
| Capacity | PASS tới **3.000 RPS**, nhưng mốc 3.000 RPS đã sát ngưỡng lỗi với error rate **0,91%**. |

## Resource Configuration

| Service       | CPU limit | Memory limit |
| ------------- | --------: | -----------: |
| postgres      |      0.50 |         512M |
| kafka         |      1.50 |        2048M |
| kafka-init    | not configured | not configured |
| opensearch    |      1.50 |        2560M |
| redis         |      1.00 |         256M |
| control-api   |      1.00 |         512M |
| ingestion-api |      1.00 |         256M |
| log-processor |      0.50 |         512M |
| nginx         |      0.50 |         128M |

## Key Observations

* `load`, `spike` và `stress` đều sạch lỗi HTTP với error rate **0,00%**.
* p95 latency của cả ba kịch bản VU cố định đều quanh **10 ms**, thấp hơn nhiều so với threshold **500 ms**.
* `spike` và `stress` cải thiện rõ rệt so với `test-002` về throughput và tail latency.
* Capacity benchmark cho thấy **2.500 RPS** là mốc sạch nhất trong nhóm tải cao.
* **2.700 RPS** vẫn là mốc upper passing tốt với error rate **0,01%**.
* **3.000 RPS** vẫn PASS nhưng nên xem là edge capacity vì error rate đạt **0,91%** và dropped iterations tăng mạnh.

## Conclusion

`test-003` cho thấy hệ thống đạt hiệu năng tốt hơn rõ rệt sau khi tăng resource trong môi trường performance. Mức vận hành sạch có thể ghi nhận quanh **2.500 RPS**, mốc **2.700 RPS** vẫn ổn định, còn **3.000 RPS** là vùng sát giới hạn theo threshold hiện tại.
