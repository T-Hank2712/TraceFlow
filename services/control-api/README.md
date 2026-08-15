# Control API

ASP.NET Core Control API quản lý phần control plane của TraceFlow.

Service này chịu trách nhiệm cho authentication, authorization, workspace, project, application, API key, search API, dashboard API, alert configuration, retention configuration và usage/quota metadata.

Control API làm việc chủ yếu với PostgreSQL cho dữ liệu nghiệp vụ và OpenSearch cho search/dashboard query. Service này không nhận log high-throughput trực tiếp; log ingestion thuộc trách nhiệm của Go Ingestion Service.
