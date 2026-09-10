using TraceFlow.Api.Domain.Common;
using TraceFlow.Api.Domain.Constants;

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

    public DateTime? ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }

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
        DateTime? expiresAt)
    {
        Id = apiKeyId;
        TraceApplicationId = traceApplicationId;
        Name = name.Trim();
        Environment = environment.Trim().ToLowerInvariant();
        KeyPrefix = keyPrefix.Trim();
        SecretHash = secretHash;
        Status = ApiKeyStatuses.Active;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        if (Status == ApiKeyStatuses.Revoked)
        {
            return;
        }

        Status = ApiKeyStatuses.Revoked;
        RevokedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkUsed()
    {
        LastUsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired(DateTime utcNow)
    {
        return ExpiresAt is not null && ExpiresAt <= utcNow;
    }

    public bool IsUsable(DateTime utcNow)
    {
        return Status == ApiKeyStatuses.Active && !IsExpired(utcNow);
    }
}