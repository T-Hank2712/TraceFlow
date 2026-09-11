namespace TraceFlow.Api.Application.ApiKeys.Queries.ListApiKeys;

public sealed record ApiKeySummaryResponse(
    Ulid Id,
    string Name,
    string Environment,
    string KeyPrefix,
    string Status,
    DateTime? ExpiresAt,
    DateTime? RevokedAt,
    DateTime? LastUsedAt,
    DateTime CreatedAt);