using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TraceFlow.Ingestion.Api.Configuration;

namespace TraceFlow.Ingestion.Api.Services.Redis;

public sealed class TenantContextCacheKey
{
    private readonly RedisOptions _redisOptions;
    public TenantContextCacheKey(IOptions<RedisOptions> redisOptions)
    {
        _redisOptions = redisOptions.Value;
    }
    public string Create(string apiKey)
    {
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(apiKey));

        var identifier = Convert.ToHexString(hash)
            .ToLowerInvariant();

        return $"{_redisOptions.TenantContextCacheKeyPrefix}:{identifier}:tenant-context";
    }
}