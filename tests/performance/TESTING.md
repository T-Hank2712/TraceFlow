# TraceFlow Performance Test — Hướng dẫn chạy

Tài liệu này tổng hợp các lệnh dùng để khởi động, kiểm tra và chạy Performance Test của TraceFlow.

## 1. Khởi động Performance Environment

Chạy từ thư mục root của project:

```bash
docker compose \
  --env-file deployments/performance/.env \
  -f deployments/performance/docker-compose.performance.yml \
  up -d
```

Nếu vừa thay đổi Dockerfile hoặc cấu hình:

```bash
docker compose \
  --env-file deployments/performance/.env \
  -f deployments/performance/docker-compose.performance.yml \
  up -d --build
```

Kiểm tra trạng thái các container:

```bash
docker compose \
  --env-file deployments/performance/.env \
  -f deployments/performance/docker-compose.performance.yml \
  ps
```

---

## 2. Kiểm tra Container

Xem toàn bộ container đang chạy:

```bash
docker ps
```

Xem log Network Simulator:

```bash
docker logs traceflow-performance-network-simulator
```

Xem log Load Generator:

```bash
docker logs traceflow-performance-load-generator
```

Xem log Nginx:

```bash
docker logs traceflow-performance-nginx
```

---

## 3. Kiểm tra Network Simulator

Kiểm tra network interface:

```bash
docker exec traceflow-performance-network-simulator ip addr
```

Kiểm tra routing table:

```bash
docker exec traceflow-performance-network-simulator ip route
```

Kiểm tra IP forwarding:

```bash
docker exec traceflow-performance-network-simulator \
  cat /proc/sys/net/ipv4/ip_forward
```

Kết quả mong đợi:

```text
1
```

Network Simulator có hai network interface:

```text
eth0 → traceflow-external-network
eth1 → traceflow-performance-network
```

---

## 4. Kiểm tra Docker Network

Liệt kê các network:

```bash
docker network ls
```

Kiểm tra External Network:

```bash
docker network inspect traceflow-external-network
```

Kiểm tra Performance Network:

```bash
docker network inspect traceflow-performance-network
```

Kiến trúc network:

```text
Load Generator
      │
      ▼
traceflow-external-network
      │
      ▼
Network Simulator
      │
      ▼
traceflow-performance-network
      │
      ▼
Nginx
```

---

## 5. Lấy IP của Nginx

Lấy IP của Nginx trên `traceflow-performance-network`:

```bash
docker inspect \
  -f '{{with index .NetworkSettings.Networks "traceflow-performance-network"}}{{.IPAddress}}{{end}}' \
  traceflow-performance-nginx
```

Có thể lưu IP vào biến:

```bash
NGINX_IP="$(
  docker inspect \
    -f '{{with index .NetworkSettings.Networks "traceflow-performance-network"}}{{.IPAddress}}{{end}}' \
    traceflow-performance-nginx
)"
```

Kiểm tra:

```bash
echo "$NGINX_IP"
```

---

## 6. Kiểm tra Load Generator → Nginx

Không thể truy cập Nginx bằng Docker hostname:

```bash
docker exec traceflow-performance-load-generator \
  curl http://traceflow-performance-nginx/control/health
```

Lý do: Load Generator và Nginx nằm trên hai Docker network khác nhau.

---

## 7. Kiểm tra Load Generator → Network Simulator

Có thể kiểm tra Network Simulator từ Load Generator:

```bash
docker exec traceflow-performance-load-generator \
  curl http://performance-gateway/control/health
```

Request này không trả về HTTP response nếu `performance-gateway` là Network Simulator, vì Network Simulator chỉ thực hiện **routing/NAT**, không phải HTTP server.

---

## 8. Kiểm tra Load Generator → Nginx thông qua Network Simulator

Lấy IP Nginx động và gửi request:

```bash
docker exec traceflow-performance-load-generator \
  curl http://$(docker inspect -f '{{with index .NetworkSettings.Networks "traceflow-performance-network"}}{{.IPAddress}}{{end}}' traceflow-performance-nginx)/control/health
```

Nếu thành công, traffic đang đi theo:

```text
Load Generator
      ↓
External Network
      ↓
Network Simulator
      ↓
Performance Network
      ↓
Nginx
      ↓
Control API
```

---

# 9. Chạy Smoke Test

Chạy:

```bash
./deployments/performance/test.sh smoke
```

Smoke test kiểm tra:

```text
GET /control/health
```

Mục đích là xác nhận toàn bộ network path từ k6 đến Control API hoạt động.

---

# 10. Chạy Single Log Test

Chạy:

```bash
./deployments/performance/test.sh single-log
```

Traffic:

```text
k6
 ↓
Load Generator
 ↓
Network Simulator
 ↓
Nginx
 ↓
Ingestion API
```

Request sử dụng API Key:

```text
Authorization: ApiKey <API_KEY>
```

API Key được truyền thông qua environment variable và không được hard-code trong k6 script.

---

# 11. Chạy k6

Thông thường không cần chạy k6 trực tiếp.

Sử dụng:

```bash
./deployments/performance/test.sh <scenario>
```

Ví dụ:

```bash
./deployments/performance/test.sh smoke
```

```bash
./deployments/performance/test.sh single-log
```

`test.sh` chịu trách nhiệm:

* Load `.env`
* Resolve IP của Nginx
* Tạo `BASE_URL`
* Truyền `API_KEY`
* Chạy container k6
* Chạy scenario tương ứng

---

# 12. Build lại Performance Environment

Sau khi thay đổi Load Generator:

```bash
docker compose \
  --env-file deployments/performance/.env \
  -f deployments/performance/docker-compose.performance.yml \
  build load-generator
```

Sau khi thay đổi Network Simulator:

```bash
docker compose \
  --env-file deployments/performance/.env \
  -f deployments/performance/docker-compose.performance.yml \
  build network-simulator
```

Hoặc build toàn bộ:

```bash
docker compose \
  --env-file deployments/performance/.env \
  -f deployments/performance/docker-compose.performance.yml \
  up -d --build
```

---

# 13. Restart Performance Environment

Restart:

```bash
docker compose \
  --env-file deployments/performance/.env \
  -f deployments/performance/docker-compose.performance.yml \
  restart
```

Dừng:

```bash
docker compose \
  --env-file deployments/performance/.env \
  -f deployments/performance/docker-compose.performance.yml \
  down
```

Dừng và xóa volume:

```bash
docker compose \
  --env-file deployments/performance/.env \
  -f deployments/performance/docker-compose.performance.yml \
  down -v
```

---

# 14. Quy trình chạy thông thường

Mỗi lần cần chạy Performance Test:

```bash
# 1. Khởi động environment
docker compose \
  --env-file deployments/performance/.env \
  -f deployments/performance/docker-compose.performance.yml \
  up -d --build

# 2. Kiểm tra container
docker compose \
  --env-file deployments/performance/.env \
  -f deployments/performance/docker-compose.performance.yml \
  ps

# 3. Chạy smoke test
./deployments/performance/test.sh smoke

# 4. Chạy single-log test
./deployments/performance/test.sh single-log
```

---

# 15. Các Scenario hiện tại

Hiện tại:

```text
smoke
single-log
```

Chạy:

```bash
./deployments/performance/test.sh smoke
```

```bash
./deployments/performance/test.sh single-log
```

Các scenario dự kiến tiếp theo:

```text
batch-log
baseline
load
stress
spike
```

Khi được thêm vào, cách chạy vẫn giữ nguyên:

```bash
./deployments/performance/test.sh <scenario>
```

---

# 16. Dừng Performance Environment

Sau khi hoàn thành:

```bash
docker compose \
  --env-file deployments/performance/.env \
  -f deployments/performance/docker-compose.performance.yml \
  down
```
