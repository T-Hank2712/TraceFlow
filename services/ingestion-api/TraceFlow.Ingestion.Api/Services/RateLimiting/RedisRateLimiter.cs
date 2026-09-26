using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TraceFlow.Ingestion.Api.Configuration;
using TraceFlow.Ingestion.Api.Contracts.RateLimiting;

namespace TraceFlow.Ingestion.Api.Services.RateLimiting;
public sealed class RedisRateLimiter: IRateLimiter
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RateLimitOptions _options;
    private const string IncrementScript = """
        local count = redis.call('INCR', KEYS[1])
        if count == 1 then
            redis.call('EXPIRE', KEYS[1], ARGV[1])
        end
        return count;
    """;
    public RedisRateLimiter(IConnectionMultiplexer redis, IOptions<RateLimitOptions> options)
    {
        _redis = redis;
        _options = options.Value;
    }
    public async Task<RateLimitResult> CheckAsync(string apiKey, CancellationToken cancellationToken)
    {
        var database = _redis.GetDatabase();
        var windowId = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / _options.WindowSeconds;
        var key = CreateRateLimitKey(apiKey, windowId);
        
        var count = (long)await database.ScriptEvaluateAsync(
            IncrementScript,
            new RedisKey[] { key },
            new RedisValue[] { _options.WindowSeconds }
        );

        var remaining = Math.Max(0, _options.PermitLimit - (int)count);

        if(count <= _options.PermitLimit)
        {
            return new RateLimitResult(
                true, _options.PermitLimit, remaining, 0
            );
        }
        
        var ttl = await database.KeyTimeToLiveAsync(key);

        var retryAfterSeconds = ttl.HasValue
            ? Math.Max(1, (int)Math.Ceiling(ttl.Value.TotalSeconds))
            : _options.WindowSeconds;

        return new RateLimitResult(
            Allowed: false,
            Limit: _options.PermitLimit,
            Remaining: 0,
            RetryAfterSeconds: retryAfterSeconds);
    }
    private string CreateRateLimitKey(string apiKey, long windowId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        var identifier = Convert.ToHexString(hash).ToLowerInvariant();
        return $"{_options.RateLimitKeyPrefix}:{identifier}:{windowId}";
    }
}