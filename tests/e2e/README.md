# End-To-End Tests

Thư mục này chứa end-to-end tests cho các luồng hoàn chỉnh của TraceFlow.

Luồng quan trọng nhất cần kiểm tra:

```text
Create Project -> Create Application -> Generate API Key -> Send Log -> Kafka -> Processor -> OpenSearch -> Search Log
```

E2E tests cần có timeout rõ ràng vì pipeline xử lý log là bất đồng bộ.

## Performance E2E Reconciliation

Kịch bản performance E2E dùng hai công cụ với hai trách nhiệm riêng:

```text
k6 -> gửi tập log xác định vào performance stack
 .NET verifier -> đối soát OpenSearch và DLQ
```

Mỗi log chứa `e2eRunId` và `e2eEventKey` duy nhất. Verifier chỉ PASS khi mọi event key được tạo bởi k6 xuất hiện đúng một lần ở OpenSearch hoặc DLQ.

Chạy từ root repository:

```bash
export API_KEY="<performance-api-key>"
tests/performance/k6/run/e2e-reconciliation.sh
```

Các biến tùy chọn:

| Variable | Default | Purpose |
| --- | ---: | --- |
| `E2E_RUN_ID` | generated value | Nhận diện duy nhất cho một lần chạy. |
| `E2E_TIMEOUT_SECONDS` | 120 | Thời gian tối đa chờ pipeline hoàn tất. |
| `E2E_K6_P95_MS` | 500 | Ngưỡng p95 của riêng phần HTTP ingestion. |

Chỉnh `VUS` và `ITERATIONS_PER_VU` trực tiếp trong `tests/performance/k6/scenarios/e2e-reconciliation.js`. Ví dụ `VUS = 20` và `ITERATIONS_PER_VU = 500` tạo chính xác 10.000 event keys. Verifier đọc hai giá trị này từ metadata của log, nên không có cấu hình workload trùng lặp trong shell script.

Kết quả PASS có nghĩa là:

```text
expected event keys = indexed event keys + DLQ event keys
missing = 0
duplicate = 0
```
