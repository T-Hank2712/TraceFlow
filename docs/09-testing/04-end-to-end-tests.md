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
