namespace TraceFlow.LogProcessor.Contracts;

public sealed record LogEvent(
    string EventId,
    string WorkspaceId,
    string ProjectId,
    string ApplicationId,
    string Environment,
    string Service,
    int Level,
    string Message,
    DateTimeOffset Timestamp,
    string? TraceId,
    string? CorrelationId,
    Dictionary<string, object?>? Metadata);
