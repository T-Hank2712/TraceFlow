namespace TraceFlow.Ingestion.Api.Contracts.Log;

public sealed record EnrichedLogEvent(
    Ulid EventId,
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid ApplicationId,
    string Environment,
    DateTimeOffset Timestamp,
    LogLevel Level,
    string Service,
    string Message,
    string? TraceId,
    string? CorrelationId,
    Dictionary<string, object?>? Metadata,
    DateTimeOffset ReceivedAt);
