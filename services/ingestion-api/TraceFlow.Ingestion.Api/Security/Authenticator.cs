namespace TraceFlow.Ingestion.Api.Security;

public sealed class Authenticator
{
    private readonly ApiKeyHeaderParser _apiKeyParser;
    private readonly IApiKeyValidator _apiKeyValidator;
    private readonly IRedisCache _redisCache;
    private readonly RedisOptions _redisOptions;
    private readonly TenantContextCacheKeyFactory _tenantContextCacheKey;
    private readonly ILogger<Authenticator> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly ApiKeyValidationLockProvider _lockProvider;

    public Authenticator(
        ApiKeyHeaderParser apiKeyParser,
        IApiKeyValidator apiKeyValidator,
        IRedisCache redisCache,
        IOptions<RedisOptions> redisOptions,
        TenantContextCacheKeyFactory tenantContextCacheKey,
        ILogger<Authenticator> logger,
        IMemoryCache memoryCache,
        ApiKeyValidationLockProvider lockProvider)
    {
        _apiKeyParser = apiKeyParser;
        _apiKeyValidator = apiKeyValidator;
        _redisCache = redisCache;
        _redisOptions = redisOptions.Value;
        _tenantContextCacheKey = tenantContextCacheKey;
        _logger = logger;
        _memoryCache = memoryCache;
        _lockProvider = lockProvider;
    }

    public async Task<Result<AuthenticatedContext>> AuthenticateAsync(
    string? authorizationHeader,
    CancellationToken cancellationToken)
    {
        var parsedKey = _apiKeyParser.Parse(authorizationHeader);

        if (!parsedKey.Success ||
            string.IsNullOrWhiteSpace(parsedKey.ApiKey))
        {
            return Result<AuthenticatedContext>.Fail(
                parsedKey.ErrorCode!,
                parsedKey.ErrorMessage!,
                StatusCodes.Status401Unauthorized);
        }

        var apiKey = parsedKey.ApiKey;
        var apiKeyFingerprint = ApiKeyFingerprint.Create(apiKey);
        var memoryCacheKey = _tenantContextCacheKey.CreateMemoryKey(apiKeyFingerprint);
        var redisCacheKey = _tenantContextCacheKey.CreateRedisKey(apiKeyFingerprint);

        var tenant = await TryGetTenantFromCacheAsync(memoryCacheKey, redisCacheKey, cancellationToken);

        if (tenant is not null)
        {
            return Result<AuthenticatedContext>.Ok(new AuthenticatedContext(apiKeyFingerprint, tenant));
        }

        var validationLock = _lockProvider.GetLock(apiKeyFingerprint);

        await validationLock.WaitAsync(cancellationToken);

        try
        {
            tenant = await TryGetTenantFromCacheAsync(memoryCacheKey, redisCacheKey, cancellationToken);

            if (tenant is not null)
            {
                return Result<AuthenticatedContext>.Ok(new AuthenticatedContext(apiKeyFingerprint, tenant));
            }

            try
            {
                tenant = await _apiKeyValidator.ValidateAsync(apiKey, cancellationToken);
            }
            catch (ControlApiUnavailableException ex)
            {
                _logger.LogError(
                    ex,
                    "Control API unavailable while validating API key.");

                return Result<AuthenticatedContext>.Fail(
                    ErrorCodes.ControlApiUnavailable,
                    "API key validation service is unavailable.",
                    StatusCodes.Status503ServiceUnavailable);
            }

            if (!tenant.Valid)
            {
                return Result<AuthenticatedContext>.Fail(
                    ErrorCodes.InvalidApiKey,
                    "API key is invalid, revoked, or expired.",
                    StatusCodes.Status401Unauthorized);
            }
            await CacheTenantAsync(
                redisCacheKey,
                memoryCacheKey,
                tenant,
                cancellationToken);

            return Result<AuthenticatedContext>.Ok(
                new AuthenticatedContext(
                    apiKeyFingerprint,
                    tenant));
        }
        finally
        {
            validationLock.Release();

            _lockProvider.ReleaseLockIfIdle(
                apiKeyFingerprint,
                validationLock);
        }
    }
    private async Task<ApiKeyValidationResult?> TryGetTenantFromCacheAsync(
        string memoryCacheKey,
        string redisCacheKey,
        CancellationToken cancellationToken
    )
    {
        if (_memoryCache.TryGetValue<ApiKeyValidationResult>(memoryCacheKey, out var cachedTenant) && cachedTenant is not null)
        {
            return cachedTenant;
        }

        try
        {
            var tenant = await _redisCache.GetAsync<ApiKeyValidationResult>(redisCacheKey, cancellationToken);
            if (tenant is not null)
            {
                _memoryCache.Set(memoryCacheKey, tenant, TimeSpan.FromSeconds(_redisOptions.TenantContextLocalCacheTtlSeconds));
            }
            return tenant;
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(
                ex,
                "Redis cache unavailable while reading tenant context. " +
                "Falling back to Control API.");
            return null;
        }
    }
    private async Task CacheTenantAsync(
        string redisCacheKey,
        string memoryCacheKey,
        ApiKeyValidationResult tenant,
        CancellationToken cancellationToken)
    {
        try
        {
            await _redisCache.SetAsync(
                redisCacheKey,
                tenant,
                TimeSpan.FromSeconds(_redisOptions.TenantContextRedisCacheTtlSeconds),
                cancellationToken
            );
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(
                ex,
                "Redis cache unavailable while storing tenant context.");
        }
        _memoryCache.Set(
            memoryCacheKey,
            tenant,
            TimeSpan.FromSeconds(
                _redisOptions.TenantContextLocalCacheTtlSeconds));
    }
}
