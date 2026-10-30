# Control API

ASP.NET Core Control API quản lý phần control plane của TraceFlow.

Service này chịu trách nhiệm cho authentication, authorization, workspace, project, application, API key, search API và internal API key validation cho Ingestion API.

Control API làm việc với PostgreSQL cho dữ liệu nghiệp vụ và OpenSearch cho search query. Service này không nhận log high-throughput trực tiếp; log ingestion thuộc trách nhiệm của Ingestion API.
