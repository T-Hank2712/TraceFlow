namespace TraceFlow.Ingestion.Api.Services.Ingestion;

public sealed class IdempotencyEventIdResolver(
    IRedisCache cache,
    IOptions<RedisOptions> options)
{
    private readonly RedisOptions _options = options.Value;

    public async Task<Ulid> ResolveAsync<T>(
        string idempotencyKey,
        int itemIndex,
        T request,
        AuthenticatedContext authentication,
        CancellationToken cancellationToken)
    {
        var requestHash = ComputeHash(JsonSerializer.Serialize(request));
        var cacheKey = CreateCacheKey(
            authentication.ApiKeyFingerprint,
            idempotencyKey,
            itemIndex);

        try
        {
            for (var attempt = 0; attempt < 2; attempt++)
            {
                var existing = await cache.GetAsync<IdempotencyRecord>(
                    cacheKey,
                    cancellationToken);

                if (existing is not null)
                {
                    return ValidateExisting(existing, requestHash);
                }

                var created = new IdempotencyRecord(Ulid.NewUlid(), requestHash);
                var added = await cache.TryAddAsync(
                    cacheKey,
                    created,
                    TimeSpan.FromSeconds(_options.IdempotencyKeyTtlSeconds),
                    cancellationToken);

                if (added)
                {
                    return created.EventId;
                }
            }

            throw new IdempotencyStoreUnavailableException(
                "Idempotency record could not be read after a concurrent write.");
        }
        catch (IdempotencyKeyReusedException)
        {
            throw;
        }
        catch (IdempotencyStoreUnavailableException)
        {
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new IdempotencyStoreUnavailableException(
                "Idempotency record could not be stored or retrieved.",
                exception);
        }
    }

    public bool IsValidKey(string? idempotencyKey)
        => !string.IsNullOrWhiteSpace(idempotencyKey) &&
           idempotencyKey.Length <= _options.IdempotencyKeyMaxLength;

    private string CreateCacheKey(
        string apiKeyFingerprint,
        string idempotencyKey,
        int itemIndex)
        => $"{_options.IdempotencyKeyPrefix}:{apiKeyFingerprint}:{ComputeHash(idempotencyKey)}:{itemIndex}";

    private static Ulid ValidateExisting(
        IdempotencyRecord existing,
        string requestHash)
    {
        if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
        {
            throw new IdempotencyKeyReusedException();
        }

        return existing.EventId;
    }

    private static string ComputeHash(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private sealed record IdempotencyRecord(Ulid EventId, string RequestHash);
}

public sealed class IdempotencyKeyReusedException : Exception;

public sealed class IdempotencyStoreUnavailableException(
    string message,
    Exception? innerException = null)
    : Exception(message, innerException);
