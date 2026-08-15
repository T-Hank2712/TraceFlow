# ADR-0002: Dùng ASP.NET Core Cho Control Plane

## Trạng thái

Chấp nhận.

## Bối cảnh

Control plane cần quản lý user, workspace, project, application, API key, search API và dashboard API.

## Quyết định

Sử dụng ASP.NET Core cho control plane.

## Lý do

- Phù hợp xây API nghiệp vụ có authentication và authorization.
- Hệ sinh thái tốt cho PostgreSQL, migration, validation và testing.
- Dễ tổ chức domain/service/repository rõ ràng.

## Hệ quả

ASP.NET Core không nhận log high-throughput trực tiếp. Log ingestion thuộc trách nhiệm của Go service.
