# Kafka Contracts

Thư mục này chứa schema cho các event được trao đổi qua Kafka.

Kafka contract là ranh giới giữa Go Ingestion Service và Go Log Processor. Mọi thay đổi schema event cần được xem như thay đổi contract vì có thể ảnh hưởng producer, consumer, test và dữ liệu index vào OpenSearch.

Event quan trọng nhất là log event sau khi ingestion đã xác thực API key và gắn thông tin tenant.
