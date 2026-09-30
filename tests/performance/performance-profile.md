# Cấu hình môi trường Performance của TraceFlow

## Mục tiêu môi trường Production

Môi trường Performance được thiết kế nhằm mô phỏng gần với cấu hình triển khai Production dự kiến:

| Tài nguyên |           Mục tiêu |
| ---------- | -----------------: |
| CPU        |             4 vCPU |
| Bộ nhớ     |               8 GB |
| Storage    | 80–100 GB SSD/NVMe |
| Swap       |               4 GB |
| Network    |            1 Gbps+ |
| OS         |         Ubuntu LTS |
| Runtime    |      Docker Engine |

## Các dịch vụ TraceFlow

Môi trường Performance bao gồm:

* Nginx
* Control API
* Ingestion API
* Log Processor
* Kafka
* OpenSearch
* PostgreSQL
* Redis

## Phân bổ tài nguyên Container ban đầu

| Service       |  CPU | Memory |
| ------------- | ---: | -----: |
| Nginx         | 0.10 |  64 MB |
| Control API   | 0.50 | 256 MB |
| Ingestion API | 0.50 | 256 MB |
| Log Processor | 0.50 | 512 MB |
| PostgreSQL    | 0.50 | 512 MB |
| Redis         | 0.25 | 128 MB |
| Kafka         | 1.00 | 1.5 GB |
| OpenSearch    | 1.50 | 2.5 GB |

## Ghi chú về phân bổ tài nguyên

Tổng giới hạn bộ nhớ của các container được cố ý duy trì thấp hơn tổng bộ nhớ mục tiêu của môi trường Production nhằm dành tài nguyên cho:

* Overhead của hệ điều hành
* Docker Engine
* Filesystem cache
* Network buffers
* JVM và native process overhead
* Các đợt tăng đột biến tài nguyên tạm thời

Các giá trị trên là **giới hạn cơ sở (baseline constraints)** cho môi trường Performance và có thể được điều chỉnh sau khi hoàn tất việc kiểm tra môi trường và thực hiện Performance Testing.

## Nguyên tắc của môi trường Performance

Môi trường phải:

1. Sử dụng cùng topology dịch vụ với Production.
2. Sử dụng Docker networks và volumes được cô lập.
3. Sử dụng cấu hình ứng dụng tương đồng với Production.
4. Áp dụng giới hạn CPU và Memory rõ ràng.
5. Đảm bảo dữ liệu Performance được tách biệt khỏi dữ liệu Development.
6. Hỗ trợ mô phỏng các điều kiện mạng.
7. Không thay đổi application code chỉ nhằm phục vụ Performance Testing.

## Ngoài phạm vi

Profile này **không xác định**:

* Capacity của hệ thống
* Throughput tối đa
* Production traffic duy trì
* Peak traffic capacity
* Các mục tiêu SLA/SLO

Các giá trị trên sẽ được xác định sau khi hoàn tất Performance Testing.
