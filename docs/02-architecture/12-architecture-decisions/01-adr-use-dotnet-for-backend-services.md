# ADR-0001: Dùng .NET Cho Backend Services

## Trạng thái

Chấp nhận.

## Bối cảnh

TraceFlow cần nhiều service backend với ranh giới rõ ràng: Control API quản lý metadata, Ingestion API nhận log và Log Processor xử lý pipeline bất đồng bộ.

## Quyết định

Sử dụng .NET cho các backend service chính: ASP.NET Core cho Control API và Ingestion API, .NET Worker Service cho Log Processor.

## Lý do

- Đồng nhất stack backend bằng C#, giúp codebase dễ đọc, dễ debug và dễ test hơn.
- ASP.NET Core phù hợp cho HTTP API có validation, authentication và structured error response.
- .NET Worker Service phù hợp cho background processing, Kafka consumer, graceful shutdown và dependency injection.
- Hệ sinh thái .NET có đủ thư viện cho PostgreSQL, Kafka, OpenSearch, validation, logging và health checks.

## Hệ quả

Project vẫn giữ kiến trúc multi-service, nhưng giảm chi phí vận hành local và onboarding bằng một stack backend thống nhất. Ranh giới giữa service vẫn được bảo vệ bằng HTTP contract, Kafka event schema và OpenSearch document contract.
