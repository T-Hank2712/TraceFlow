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
    private readonly ApiKeyValidationFlightStore _validationFlights;

    public Authenticator(
        ApiKeyHeaderParser apiKeyParser,
        IApiKeyValidator apiKeyValidator,
        IRedisCache redisCache,
        IOptions<RedisOptions> redisOptions,
        TenantContextCacheKeyFactory tenantContextCacheKey,
        ILogger<Authenticator> logger,
        IMemoryCache memoryCache,
        ApiKeyValidationFlightStore validationFlights)
    {
        _apiKeyParser = apiKeyParser;
        _apiKeyValidator = apiKeyValidator;
        _redisCache = redisCache;
        _redisOptions = redisOptions.Value;
        _tenantContextCacheKey = tenantContextCacheKey;
        _logger = logger;
        _memoryCache = memoryCache;
        _validationFlights = validationFlights;
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

        var canUseTenantCache = true;

        if (_apiKeyParser.TryGetApiKeyId(apiKey, out var apiKeyId))
        {
            var revocationCheck = await IsApiKeyRevokedAsync(apiKeyId, cancellationToken);

            if (revocationCheck is true)
            {
                return Result<AuthenticatedContext>.Fail(
                    ErrorCodes.InvalidApiKey,
                    "API key is invalid, revoked, or expired.",
                    StatusCodes.Status401Unauthorized);
            }

            canUseTenantCache = revocationCheck is false;
        }

        var apiKeyFingerprint = ApiKeyFingerprint.Create(apiKey);
        var memoryCacheKey = _tenantContextCacheKey.CreateMemoryKey(apiKeyFingerprint);
        var redisCacheKey = _tenantContextCacheKey.CreateRedisKey(apiKeyFingerprint);

        var tenant = canUseTenantCache
            ? await TryGetTenantFromCacheAsync(memoryCacheKey, redisCacheKey, cancellationToken)
            : null;

        if (tenant is not null)
        {
            return Result<AuthenticatedContext>.Ok(new AuthenticatedContext(apiKeyFingerprint, tenant));
        }

        try
        {
            if (!canUseTenantCache)
            {
                tenant = await ValidateAndCacheTenantAsync(
                    apiKey,
                    redisCacheKey,
                    memoryCacheKey,
                    cancellationToken);
            }
            else
            {
                tenant = await _validationFlights.RunAsync(
                    apiKeyFingerprint,
                    async () =>
                    {
                        var cachedTenant = await TryGetTenantFromCacheAsync(
                            memoryCacheKey,
                            redisCacheKey,
                            CancellationToken.None);

                        if (cachedTenant is not null)
                        {
                            return cachedTenant;
                        }

                        return await ValidateAndCacheTenantAsync(
                            apiKey,
                            redisCacheKey,
                            memoryCacheKey,
                            CancellationToken.None);
                    },
                    cancellationToken);
            }
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

        return Result<AuthenticatedContext>.Ok(
            new AuthenticatedContext(
                apiKeyFingerprint,
                tenant));
    }

    private async Task<bool?> IsApiKeyRevokedAsync(
        Ulid apiKeyId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _redisCache.ExistsAsync(
                _tenantContextCacheKey.CreateRevocationKey(apiKeyId),
                cancellationToken);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(
                ex,
                "Redis unavailable while checking API key revocation. " +
                "Bypassing tenant context cache and validating with Control API.");
            return null;
        }
    }

    private async Task<ApiKeyValidationResult> ValidateAndCacheTenantAsync(
        string apiKey,
        string redisCacheKey,
        string memoryCacheKey,
        CancellationToken cancellationToken)
    {
        var validatedTenant = await _apiKeyValidator.ValidateAsync(
            apiKey,
            cancellationToken);

        if (GetCacheExpiration(
                validatedTenant,
                _redisOptions.TenantContextRedisCacheTtlSeconds) is not null)
        {
            await CacheTenantAsync(
                redisCacheKey,
                memoryCacheKey,
                validatedTenant,
                cancellationToken);
        }

        return validatedTenant;
    }

    private async Task<ApiKeyValidationResult?> TryGetTenantFromCacheAsync(
        string memoryCacheKey,
        string redisCacheKey,
        CancellationToken cancellationToken
    )
    {
        if (_memoryCache.TryGetValue<ApiKeyValidationResult>(memoryCacheKey, out var cachedTenant) && cachedTenant is not null)
        {
            if (IsTenantContextUsable(cachedTenant))
            {
                return cachedTenant;
            }

            _memoryCache.Remove(memoryCacheKey);
        }

        try
        {
            var tenant = await _redisCache.GetAsync<ApiKeyValidationResult>(redisCacheKey, cancellationToken);
            if (tenant is not null && IsTenantContextUsable(tenant))
            {
                var memoryCacheExpiration = GetCacheExpiration(
                    tenant,
                    _redisOptions.TenantContextLocalCacheTtlSeconds);

                if (memoryCacheExpiration is not null)
                {
                    _memoryCache.Set(memoryCacheKey, tenant, memoryCacheExpiration.Value);
                }

                return tenant;
            }

            return null;
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
        var redisCacheExpiration = GetCacheExpiration(
            tenant,
            _redisOptions.TenantContextRedisCacheTtlSeconds);
        var memoryCacheExpiration = GetCacheExpiration(
            tenant,
            _redisOptions.TenantContextLocalCacheTtlSeconds);

        if (redisCacheExpiration is null || memoryCacheExpiration is null)
        {
            return;
        }

        try
        {
            await _redisCache.SetAsync(
                redisCacheKey,
                tenant,
                redisCacheExpiration.Value,
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
            memoryCacheExpiration.Value);
    }

    private static bool IsTenantContextUsable(ApiKeyValidationResult tenant)
    {
        return tenant.Valid &&
            tenant.ExpiresAt is not null &&
            tenant.ExpiresAt.Value > DateTimeOffset.UtcNow;
    }

    private static TimeSpan? GetCacheExpiration(
        ApiKeyValidationResult tenant,
        int configuredTtlSeconds)
    {
        if (!IsTenantContextUsable(tenant))
        {
            return null;
        }

        var remainingLifetime = tenant.ExpiresAt!.Value - DateTimeOffset.UtcNow;
        var configuredTtl = TimeSpan.FromSeconds(configuredTtlSeconds);

        return remainingLifetime < configuredTtl
            ? remainingLifetime
            : configuredTtl;
    }
}
