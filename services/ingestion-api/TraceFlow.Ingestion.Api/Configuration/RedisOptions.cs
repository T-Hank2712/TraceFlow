namespace TraceFlow.Ingestion.Api.Configuration;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public required string ConnectionString { get; init; }
    public int TenantContextRedisCacheTtlSeconds { get; init; }
    public required string TenantContextRedisCacheKeyPrefix { get; init; }
    public int TenantContextLocalCacheTtlSeconds { get; init; }
    public required string TenantContextLocalCacheKeyPrefix { get; init; }
    public required string ApiKeyRevocationKeyPrefix { get; init; }
    public int IdempotencyKeyTtlSeconds { get; init; } = 86400;
    public int IdempotencyKeyMaxLength { get; init; } = 128;
    public string IdempotencyKeyPrefix { get; init; } = "traceflow:idempotency";
}
