namespace TraceFlow.Ingestion.Api.Security;

public sealed class Authenticator
{
    private readonly ApiKeyHeaderParser _apiKeyParser;
    private readonly IApiKeyValidator _apiKeyValidator;
    private readonly IRedisCache _redisCache;
    private readonly RedisOptions _redisOptions;
    private readonly TenantContextCacheKey _tenantContextCacheKey;
    private readonly ILogger<Authenticator> _logger;
    private readonly IMemoryCache _memoryCache;

    public Authenticator(
        ApiKeyHeaderParser apiKeyParser,
        IApiKeyValidator apiKeyValidator,
        IRedisCache redisCache,
        IOptions<RedisOptions> redisOptions,
        TenantContextCacheKey tenantContextCacheKey,
        ILogger<Authenticator> logger,
        IMemoryCache memoryCache)
    {
        _apiKeyParser = apiKeyParser;
        _apiKeyValidator = apiKeyValidator;
        _redisCache = redisCache;
        _redisOptions = redisOptions.Value;
        _tenantContextCacheKey = tenantContextCacheKey;
        _logger = logger;
        _memoryCache = memoryCache;
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
        var memoryCacheKey = CreateMemoryCacheKey(apiKey);

        if (_memoryCache.TryGetValue<ApiKeyValidationResult>(
                memoryCacheKey,
                out var cachedTenant) &&
            cachedTenant is not null)
        {
            return Result<AuthenticatedContext>.Ok(
                new AuthenticatedContext(
                    apiKey,
                    cachedTenant));
        }

        var cacheKey = _tenantContextCacheKey.Create(apiKey);

        ApiKeyValidationResult? tenant = null;

        try
        {
            tenant = await _redisCache.GetAsync<ApiKeyValidationResult>(
                cacheKey,
                cancellationToken);

            if (tenant is not null)
            {
                _memoryCache.Set(
                    memoryCacheKey,
                    tenant,
                    TimeSpan.FromSeconds(
                        _redisOptions.TenantContextMemoryCacheTtlSeconds));
            }
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(
                ex,
                "Redis cache unavailable while reading tenant context. " +
                "Falling back to Control API.");
        }

        if (tenant is null)
        {
            _logger.LogInformation(
                "Tenant context cache miss. Validating API key through Control API.");

            try
            {
                tenant = await _apiKeyValidator.ValidateAsync(
                    apiKey,
                    cancellationToken);
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

            try
            {
                await _redisCache.SetAsync(
                    cacheKey,
                    tenant,
                    TimeSpan.FromSeconds(
                        _redisOptions.TenantContextCacheTtlSeconds),
                    cancellationToken);
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
                    _redisOptions.TenantContextMemoryCacheTtlSeconds));
        }

        return Result<AuthenticatedContext>.Ok(
            new AuthenticatedContext(
                apiKey,
                tenant));
    }
    private string CreateMemoryCacheKey(string apiKey)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        var identifier = Convert.ToHexString(hash).ToLowerInvariant();
        return $"{_redisOptions.TenantContextMemoryCacheKeyPrefix}:{identifier}";
    }
}
