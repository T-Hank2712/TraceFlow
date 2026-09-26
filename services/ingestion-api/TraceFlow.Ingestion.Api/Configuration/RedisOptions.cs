namespace TraceFlow.Ingestion.Api.Configuration;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public required string ConnectionString { get; init; }
    public int TenantContextCacheTtlSeconds { get; init; }
    public required string TenantContextCacheKeyPrefix { get; init; }
}