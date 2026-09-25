using System.Text.Json;
using StackExchange.Redis;

namespace TraceFlow.Ingestion.Api.Services.Redis;

public sealed class RedisCache(
    IConnectionMultiplexer connection)
    : IRedisCache
{
    private readonly IDatabase _database = connection.GetDatabase();

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var serialized = JsonSerializer.Serialize(value);

        await _database.StringSetAsync(
            key,
            serialized,
            expiration);
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync(key);
    }
}