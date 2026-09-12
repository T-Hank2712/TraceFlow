namespace TraceFlow.Api.Domain.Dtos.Logs;

public sealed record LogSearchItemResponse(
    string EventId,
    DateTime Timestamp,
    DateTime ReceivedAt,
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid ApplicationId,
    string Environment,
    string Service,
    string Level,
    string Message,
    string? TraceId,
    string? CorrelationId,
    Dictionary<string, object>? Metadata);