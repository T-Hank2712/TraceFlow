namespace TraceFlow.Ingestion.Api.Services.Redis;

public interface IRedisCache
{
    Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default);
}