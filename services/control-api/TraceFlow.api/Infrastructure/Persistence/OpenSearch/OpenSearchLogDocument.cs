namespace TraceFlow.Api.Infrastructure.Persistence.OpenSearch;

public sealed class OpenSearchLogDocument
{
    [JsonPropertyName("eventId")]
    public string EventId { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; init; }

    [JsonPropertyName("receivedAt")]
    public DateTimeOffset ReceivedAt { get; init; }

    [JsonPropertyName("workspaceId")]
    public Ulid WorkspaceId { get; init; }

    [JsonPropertyName("projectId")]
    public Ulid ProjectId { get; init; }

    [JsonPropertyName("applicationId")]
    public Ulid ApplicationId { get; init; }

    [JsonPropertyName("environment")]
    public string Environment { get; init; } = string.Empty;

    [JsonPropertyName("service")]
    public string Service { get; init; } = string.Empty;

    [JsonPropertyName("level")]
    public LogLevel Level { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("traceId")]
    public string? TraceId { get; init; }

    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; init; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}
