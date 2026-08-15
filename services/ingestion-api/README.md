# Ingestion API

Go Ingestion API là entrypoint để application gửi log vào TraceFlow.

Service này xác thực API key, validate payload, chuẩn hóa log event, gắn thông tin project/application và publish event vào Kafka. API trả response nhanh sau khi log được nhận vào pipeline theo policy.

Ingestion API không index trực tiếp vào OpenSearch và không xử lý nghiệp vụ control plane như workspace, project hoặc user management.
