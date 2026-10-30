# Configuration

TraceFlow cấu hình qua environment variables.

## Nhóm cấu hình

- Database.
- Kafka.
- OpenSearch.
- JWT.
- API key hashing.
- Rate limiting.
- Trusted proxy networks.
- Redis tenant-context cache, API-key revoke marker, idempotency và rate limit.

## Ingestion reliability settings

- `TrustedProxies__KnownNetworks__0`: CIDR của Nginx/reverse proxy được phép ảnh hưởng `X-Forwarded-For`. Không tin forward header từ network không nằm trong danh sách.
- `Redis__ApiKeyRevocationKeyPrefix`: prefix marker revoke được Control API ghi và Ingestion API kiểm tra trước cache.
- `Redis__IdempotencyKeyTtlSeconds`: thời gian giữ ánh xạ idempotency, mặc định 86.400 giây.
- `Redis__IdempotencyKeyMaxLength`: độ dài tối đa `Idempotency-Key`, mặc định 128.
- `Redis__IdempotencyKeyPrefix`: prefix Redis của idempotency record.
- `OpenSearch__Index`: index Log Processor tạo/xác minh với `dynamic: false` và `metadata: flat_object`.
- `OpenSearch__SkipTlsVerify`: chỉ bật trong local/performance khi dùng certificate không được tin cậy.

## Nguyên tắc

Mỗi biến môi trường phải có giá trị mẫu trong `.env.example` và tài liệu mô tả ý nghĩa, default, yêu cầu bảo mật. Không commit file `.env` hoặc `.env.performance` chứa secret thật.
