namespace TraceFlow.Ingestion.Api.Configuration;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public required string ConnectionString { get; init; }
    public int TenantContextRedisCacheTtlSeconds { get; init; }
    public required string TenantContextCacheKeyPrefix { get; init; }
    public int TenantContextLocalCacheTtlSeconds { get; init; }
    public required string TenantContextMemoryCacheKeyPrefix { get; init; }
}
