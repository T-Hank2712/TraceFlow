namespace TraceFlow.Api.Domain.Entities;

public class ApiKey : Entity
{
    public Ulid TraceApplicationId { get; private set; }
    public TraceApplication TraceApplication { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;
    public string Environment { get; private set; } = string.Empty;
    public string KeyPrefix { get; private set; } = string.Empty;
    public string SecretHash { get; private set; } = string.Empty;
    public string Status { get; private set; } = ApiKeyStatuses.Active;

    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public DateTimeOffset? LastUsedAt { get; private set; }

    private ApiKey()
    {
    }

    public ApiKey(
        Ulid apiKeyId,
        Ulid traceApplicationId,
        string name,
        string environment,
        string keyPrefix,
        string secretHash,
        DateTimeOffset expiresAt,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = apiKeyId;
        TraceApplicationId = traceApplicationId;
        Name = name.Trim();
        Environment = environment.Trim().ToLowerInvariant();
        KeyPrefix = keyPrefix.Trim();
        SecretHash = secretHash;
        Status = ApiKeyStatuses.Active;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public void Revoke(DateTimeOffset revokedAt)
    {
        if (Status == ApiKeyStatuses.Revoked)
        {
            return;
        }

        Status = ApiKeyStatuses.Revoked;
        RevokedAt = revokedAt;
        UpdatedAt = revokedAt;
    }

    public void MarkUsed(DateTimeOffset usedAt)
    {
        LastUsedAt = usedAt;
        UpdatedAt = usedAt;
    }

    public bool IsExpired(DateTimeOffset utcNow)
    {
        return ExpiresAt <= utcNow;
    }

    public bool IsUsable(DateTimeOffset utcNow)
    {
        return Status == ApiKeyStatuses.Active && !IsExpired(utcNow);
    }
}
