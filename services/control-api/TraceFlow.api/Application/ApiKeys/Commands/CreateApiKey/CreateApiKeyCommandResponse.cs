namespace TraceFlow.Api.Application.ApiKeys.Commands.CreateApiKey;

public sealed record CreateApiKeyResponse(
    Ulid Id,
    string Name,
    string Environment,
    string KeyPrefix,
    string Secret,
    string Status,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt);
