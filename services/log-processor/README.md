# Log Processor

Go Log Processor xử lý log event từ Kafka và index vào OpenSearch.

Worker này consume Kafka theo consumer group, validate internal event, enrich dữ liệu, gom batch, bulk index vào OpenSearch, xử lý retry, DLQ và graceful shutdown.

Processor là thành phần chính đảm bảo dữ liệu log đi từ stream sang search backend một cách ổn định, có kiểm soát và có khả năng quan sát.
