using TraceFlow.Ingestion.Api.Contracts;

namespace TraceFlow.Ingestion.Api.Ingestion;

public sealed record IngestLogResult(
    bool Success,
    IngestLogResponse? Response,
    ErrorResponse? Error,
    int StatusCode);