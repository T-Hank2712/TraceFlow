# TraceFlow Performance Test - Hướng Dẫn Chạy

Tài liệu này mô tả cách khởi động môi trường performance, kiểm tra readiness và chạy các kịch bản k6 hiện có của TraceFlow.

## 1. Phạm Vi

Performance stack dùng để đo luồng:

```text
k6
  -> load-generator
  -> network-simulator
  -> nginx
  -> ingestion/control api
  -> kafka
  -> log processor
  -> opensearch
```

Các lệnh bên dưới chạy từ thư mục root của repository.

## 2. Chuẩn Bị Env

Tạo file env performance nếu chưa có:

```bash
cp deployments/performance/.env.performance.example \
  deployments/performance/.env.performance
```

Kiểm tra các nhóm biến quan trọng trong `deployments/performance/.env.performance`:

- `POSTGRES_PASSWORD`, `REDIS_PASSWORD`, `OPENSEARCH_INITIAL_ADMIN_PASSWORD`
- `Jwt__Secret`, `ControlApi__InternalServiceSecret`
- `Kafka__*`, `Redis__*`, `RateLimiting__*`, `Ingestion__*`

Không commit file `.env.performance` thật.

## 3. Khởi Động Performance Stack

Khởi động toàn bộ stack:

```bash
docker compose \
  --env-file deployments/performance/.env.performance \
  -f deployments/performance/docker-compose.performance.yml \
  up -d --build
```

Nếu không cần build lại image:

```bash
docker compose \
  --env-file deployments/performance/.env.performance \
  -f deployments/performance/docker-compose.performance.yml \
  up -d
```

Kiểm tra container:

```bash
docker compose \
  --env-file deployments/performance/.env.performance \
  -f deployments/performance/docker-compose.performance.yml \
  ps
```

## 4. Kiểm Tra Health

Nginx được expose ra máy host tại port `8080`.

Kiểm tra Control API:

```bash
curl http://localhost:8080/control/health
```

Kiểm tra Ingestion API:

```bash
curl http://localhost:8080/ingestion/health/live
curl http://localhost:8080/ingestion/health/ready
```

Kết quả mong đợi:

```text
Healthy
```

Nếu `ready` không healthy, kiểm tra Kafka, Redis, Control API hoặc các biến env bắt buộc.

## 5. Chuẩn Bị API Key

Các kịch bản ingestion cần API key thật:

```bash
export API_KEY="<your-api-key>"
```

API key được truyền vào k6 bằng header:

```text
Authorization: ApiKey <API_KEY>
```

Không hard-code API key trong script.

## 6. Chạy Smoke Test

Smoke test kiểm tra đường đi cơ bản tới Control API:

```bash
tests/performance/k6/run/baseline.sh smoke
```

Kịch bản này gọi:

```text
GET /control/health
```

## 7. Chạy Single Log Test

Chạy một request ingest log:

```bash
tests/performance/k6/run/single-log.sh single-log
```

Kịch bản này gọi:

```text
POST /ingestion/v1/logs
```

## 8. Chạy Batch Log Test

Chạy một request batch ingest:

```bash
export BATCH_SIZE=10
tests/performance/k6/run/batch-log.sh batch-log
```

Kịch bản này gọi:

```text
POST /ingestion/v1/batch-logs
```

Nếu không set `BATCH_SIZE`, script sẽ dùng mặc định `10`.

## 9. Chạy Baseline, Load, Spike, Stress

Baseline:

```bash
tests/performance/k6/run/baseline.sh baseline
```

Load:

```bash
tests/performance/k6/run/load.sh
```

Spike:

```bash
tests/performance/k6/run/spike.sh
```

Stress:

```bash
tests/performance/k6/run/stress.sh
```

Các kịch bản này đều cần `API_KEY`.

## 10. Chạy E2E Marker Test

E2E performance test gửi một log có marker để dễ truy vết trong pipeline:

```bash
export TEST_MARKER="traceflow-e2e-$(date +%Y%m%d-%H%M%S)"
tests/performance/k6/run/e2e.sh e2e
```

Sau khi chạy, có thể dùng marker để kiểm tra log đã đi qua Kafka, Log Processor và OpenSearch.

## 11. Chạy E2E Reconciliation

Kịch bản này gửi workload xác định và tự đối soát dữ liệu sau pipeline:

```bash
export API_KEY="<your-api-key>"
tests/performance/k6/run/e2e-reconciliation.sh
```

Scenario hiện gửi 50 VUs x 1.000 iterations = 50.000 log, yêu cầu không có HTTP failure và p95 dưới 15 ms. Sau k6, `TraceFlow.PipelineVerifier` tìm event bằng `E2E_RUN_ID` và kiểm tra OpenSearch cùng DLQ. Cả k6 và verifier phải thành công thì run mới đạt.

Không dùng lại `E2E_RUN_ID` nếu muốn một run độc lập. Có thể đặt `E2E_TIMEOUT_SECONDS` để tăng thời gian chờ verifier; biến `E2E_K6_P95_MS` hiện chưa thay đổi threshold vì threshold được cố định trong scenario.

## 12. Cách Script Hoạt Động

Các script trong `tests/performance/k6/run` sẽ:

1. Đọc Docker Compose file tại `deployments/performance/docker-compose.performance.yml`.
2. Dùng env file `deployments/performance/.env.performance`.
3. Resolve IP của container `traceflow-performance-nginx` trong `traceflow-performance-network`.
4. Tạo `BASE_URL=http://<nginx-ip>`.
5. Chạy container `k6` bằng Docker Compose.
6. Truyền `BASE_URL`, `API_KEY` và các biến scenario vào k6.

Không cần chạy `k6` trực tiếp trên máy host.

## 13. Xem Log Khi Lỗi

Nginx:

```bash
docker logs traceflow-performance-nginx
```

Control API:

```bash
docker logs traceflow-performance-control-api
```

Ingestion API:

```bash
docker logs traceflow-performance-ingestion-api
```

Kafka:

```bash
docker logs traceflow-performance-kafka
```

Log Processor:

```bash
docker logs traceflow-performance-log-processor
```

OpenSearch:

```bash
docker logs traceflow-performance-opensearch
```

## 14. Kiểm Tra Network Path

Lấy IP Nginx trong performance network:

```bash
docker inspect \
  -f '{{with index .NetworkSettings.Networks "traceflow-performance-network"}}{{.IPAddress}}{{end}}' \
  traceflow-performance-nginx
```

Kiểm tra từ load-generator tới Nginx qua network simulator:

```bash
docker exec traceflow-performance-load-generator \
  curl http://$(docker inspect -f '{{with index .NetworkSettings.Networks "traceflow-performance-network"}}{{.IPAddress}}{{end}}' traceflow-performance-nginx)/control/health
```

Traffic mong đợi:

```text
load-generator
  -> network-simulator
  -> nginx
  -> service
```

## 15. Dừng Hoặc Reset Stack

Dừng stack:

```bash
docker compose \
  --env-file deployments/performance/.env.performance \
  -f deployments/performance/docker-compose.performance.yml \
  down
```

Dừng và xóa volume performance:

```bash
docker compose \
  --env-file deployments/performance/.env.performance \
  -f deployments/performance/docker-compose.performance.yml \
  down -v
```

## 16. Quy Trình Khuyến Nghị

Chạy theo thứ tự:

```bash
docker compose \
  --env-file deployments/performance/.env.performance \
  -f deployments/performance/docker-compose.performance.yml \
  up -d --build

curl http://localhost:8080/control/health
curl http://localhost:8080/ingestion/health/ready

export API_KEY="<your-api-key>"

tests/performance/k6/run/baseline.sh smoke
tests/performance/k6/run/single-log.sh single-log
tests/performance/k6/run/batch-log.sh batch-log
tests/performance/k6/run/baseline.sh baseline
tests/performance/k6/run/load.sh
```

Sau khi baseline ổn mới chạy:

```bash
tests/performance/k6/run/spike.sh
tests/performance/k6/run/stress.sh
```

## 17. Ghi Nhận Kết Quả

Sau mỗi lần chạy, nên ghi lại:

- commit SHA
- branch
- env/profile đã dùng
- scenario
- VUs/duration/batch size
- request rate
- `http_req_duration` p50/p95/p99
- `http_req_failed`
- CPU/memory container chính
- lỗi đáng chú ý trong service logs

Kết quả benchmark nên lưu trong:

```text
tests/performance/results/<test-id>/
```

## 18. Troubleshooting Nhanh

Nếu k6 báo thiếu API key:

```bash
export API_KEY="<your-api-key>"
```

Nếu `ready` unhealthy:

```bash
docker logs traceflow-performance-ingestion-api
docker logs traceflow-performance-kafka
docker logs traceflow-performance-redis
```

Nếu k6 không resolve được Nginx IP:

```bash
docker ps | grep traceflow-performance-nginx
docker network inspect traceflow-performance-network
```

Nếu scenario trả `401`:

- API key sai, revoked hoặc hết hạn.
- API key không thuộc tenant/application hợp lệ.

Nếu scenario trả `429`:

- Đã chạm rate limit.
- Giảm VUs hoặc tăng `RateLimiting__PermitLimit` trong `.env.performance`.

Nếu scenario trả `503`:

- Kafka publish lỗi.
- Control API validation timeout/unavailable.
- Redis rate limit unavailable.
