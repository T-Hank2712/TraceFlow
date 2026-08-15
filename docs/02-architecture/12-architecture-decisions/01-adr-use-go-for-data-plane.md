# ADR-0001: Dùng Go Cho Data Plane

## Trạng thái

Chấp nhận.

## Bối cảnh

Data plane cần nhận log, publish Kafka, consume Kafka, xử lý batch và chịu tải network/concurrency tốt.

## Quyết định

Sử dụng Go cho Go Ingestion Service và Go Log Processor.

## Lý do

- Goroutine và channel phù hợp xử lý đồng thời.
- Runtime nhẹ.
- Dễ viết service network đơn giản, nhanh.
- Phù hợp với Kafka producer/consumer và worker pool.

## Hệ quả

Project cần quản lý hai stack ngôn ngữ: Go và C#. Ranh giới contract giữa hai stack phải rõ ràng qua API, Kafka event schema và database contract.
