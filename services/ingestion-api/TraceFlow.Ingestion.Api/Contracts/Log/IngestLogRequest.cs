namespace TraceFlow.Ingestion.Api.Contracts.Log;

public sealed record IngestLogRequest(
    DateTimeOffset? Timestamp,
    LogLevel Level,
    string Service,
    string Message,
    string? TraceId,
    string? CorrelationId,
    Dictionary<string, object?>? Metadata
);
