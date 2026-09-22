namespace TraceFlow.LogProcessor.Contracts;

public sealed record LogEvent(
    Ulid EventId,
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid ApplicationId,
    string Environment,
    string Service,
    int Level,
    string Message,
    DateTimeOffset Timestamp,
    string? TraceId,
    string? CorrelationId,
    Dictionary<string, object?>? Metadata);