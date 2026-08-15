# Services

Thư mục này chứa các service runtime của TraceFlow.

TraceFlow chia service theo ranh giới trách nhiệm rõ ràng:

- `control-api/`: ASP.NET Core service cho control plane.
- `ingestion-api/`: Go service nhận log từ application và publish vào Kafka.
- `log-processor/`: Go worker consume Kafka, xử lý batch và index vào OpenSearch.

Mỗi service có README riêng để mô tả trách nhiệm, dependency và phạm vi không được vượt qua.
