namespace TraceFlow.Api.Application.ApiKeys.Commands.RevokeApiKey;

public sealed record RevokeApiKeyResponse(
    Ulid Id,
    string Name,
    string Environment,
    string KeyPrefix,
    string Status,
    DateTime? RevokedAt,
    DateTime UpdatedAt);