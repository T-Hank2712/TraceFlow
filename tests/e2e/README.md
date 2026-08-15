# End-To-End Tests

Thư mục này chứa end-to-end tests cho các luồng hoàn chỉnh của TraceFlow.

Luồng quan trọng nhất cần kiểm tra:

```text
Create Project -> Create Application -> Generate API Key -> Send Log -> Kafka -> Processor -> OpenSearch -> Search Log
```

E2E tests cần có timeout rõ ràng vì pipeline xử lý log là bất đồng bộ.
