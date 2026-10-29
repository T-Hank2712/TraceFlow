namespace TraceFlow.Api.Infrastructure.Redis;

public sealed class RedisApiKeyRevocationStore(
    IConnectionMultiplexer connection,
    IOptions<RedisOptions> options)
    : IApiKeyRevocationStore
{
    private readonly IDatabase _database = connection.GetDatabase();
    private readonly RedisOptions _options = options.Value;

    public async Task RevokeAsync(
        Ulid apiKeyId,
        CancellationToken cancellationToken)
    {
        await _database.StringSetAsync(
            CreateKey(apiKeyId),
            "revoked");
    }

    private string CreateKey(Ulid apiKeyId)
    {
        return $"{_options.ApiKeyRevocationKeyPrefix}:{apiKeyId}";
    }
}
