# End-To-End Tests

End-to-end test kiểm tra luồng hoàn chỉnh từ client gửi log tới search được log.

## Flow chính

```text
Create Workspace
-> Create Project
-> Create Application
-> Generate API Key
-> Send Log
-> Kafka
-> Processor
-> OpenSearch
-> Search Log
```

## Nguyên tắc

E2E test phải có timeout rõ ràng vì pipeline bất đồng bộ có độ trễ.

## E2E reconciliation workload

Kịch bản `tests/performance/k6/run/e2e-reconciliation.sh` gửi workload xác định vào performance stack rồi chạy `TraceFlow.PipelineVerifier` để đối soát toàn pipeline.

- k6 gửi 50 VUs, mỗi VU 1.000 log: tổng cộng 50.000 event, mỗi event có `e2eRunId` và `e2eEventKey` riêng.
- Mỗi request có `Idempotency-Key` bằng event key; request chỉ được tính thành công khi HTTP `200` và response có `eventId`.
- Ngưỡng hiện tại của scenario là `http_req_failed == 0`, mọi check thành công và `http_req_duration p95 < 15 ms`.
- Verifier tìm workload theo `E2E_RUN_ID`, chờ pipeline hoàn tất rồi đối soát event ở OpenSearch và Kafka DLQ để phát hiện thiếu, trùng hoặc thất lạc log.

Chạy test với API key hợp lệ:

```bash
export API_KEY="<your-api-key>"
tests/performance/k6/run/e2e-reconciliation.sh
```

Kết quả E2E chỉ hợp lệ khi cả k6 và verifier đều exit code `0`.
