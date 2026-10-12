namespace TraceFlow.Ingestion.Api.Services.Redis;

public sealed class TenantContextCacheKeyFactory
{
    private readonly RedisOptions _redisOptions;
    public TenantContextCacheKeyFactory(IOptions<RedisOptions> redisOptions)
    {
        _redisOptions = redisOptions.Value;
    }
    public string CreateRedisKey(string apiKeyFingerprint)
    {
        return $"{_redisOptions.TenantContextRedisCacheKeyPrefix}:{apiKeyFingerprint}:tenant-context";
    }
    public string CreateMemoryKey(string apiKeyFingerprint)
    {
        return $"{_redisOptions.TenantContextLocalCacheKeyPrefix}:{apiKeyFingerprint}";
    }
}
