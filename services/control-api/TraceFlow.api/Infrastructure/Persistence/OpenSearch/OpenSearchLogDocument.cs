using System.Text.Json.Serialization;

namespace TraceFlow.Api.Infrastructure.OpenSearch;

public sealed class OpenSearchLogDocument
{
    [JsonPropertyName("eventId")]
    public string EventId { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }

    [JsonPropertyName("receivedAt")]
    public DateTime ReceivedAt { get; init; }

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
    public string Level { get; init; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("traceId")]
    public string? TraceId { get; init; }

    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; init; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}