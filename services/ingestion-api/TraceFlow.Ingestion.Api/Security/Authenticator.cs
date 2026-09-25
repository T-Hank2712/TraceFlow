using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TraceFlow.Ingestion.Api.Clients;
using TraceFlow.Ingestion.Api.Configurations;
using TraceFlow.Ingestion.Api.Contracts;
using TraceFlow.Ingestion.Api.Errors;
using TraceFlow.Ingestion.Api.Services.Ingestion;
using TraceFlow.Ingestion.Api.Services.Redis;

namespace TraceFlow.Ingestion.Api.Security;

public sealed class Authenticator
{
    private readonly ApiKeyHeaderParser _apiKeyParser;
    private readonly IApiKeyValidator _apiKeyValidator;
    private readonly IRedisCache _redisCache;
    private readonly RedisOptions _redisOptions;
    private readonly TenantContextCacheKey _tenantContextCacheKey;
    private readonly ILogger _logger;

    public Authenticator(
        ApiKeyHeaderParser apiKeyParser,
        IApiKeyValidator apiKeyValidator,
        IRedisCache redisCache,
        IOptions<RedisOptions> redisOptions,
        TenantContextCacheKey tenantContextCacheKey,
        ILogger<Authenticator> logger)
    {
        _apiKeyParser = apiKeyParser;
        _apiKeyValidator = apiKeyValidator;
        _redisCache = redisCache;
        _redisOptions = redisOptions.Value;
        _tenantContextCacheKey = tenantContextCacheKey;
        _logger = logger;
    }

    public async Task<Result<ApiKeyValidationResult>> AuthenticateAsync(
        string? authorizationHeader,
        CancellationToken cancellationToken)
    {
        var parsedKey = _apiKeyParser.Parse(authorizationHeader);

        if (!parsedKey.Success || string.IsNullOrWhiteSpace(parsedKey.ApiKey))
        {
            return Result<ApiKeyValidationResult>.Fail(
                parsedKey.ErrorCode!,
                parsedKey.ErrorMessage!,
                StatusCodes.Status401Unauthorized);
        }

        var apiKey = parsedKey.ApiKey;

        var cacheKey = _tenantContextCacheKey.Create(apiKey);

        try
        {
            var cached = await _redisCache.GetAsync<ApiKeyValidationResult>(
                cacheKey,
                cancellationToken);

            if (cached is not null)
            {
                return Result<ApiKeyValidationResult>.Ok(cached);
            }
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis cache unavailable while validating API key. Falling back to Control API.");
        }

        var tenant = await _apiKeyValidator.ValidateAsync(
            apiKey,
            cancellationToken);

        if (!tenant.Valid)
        {
            return Result<ApiKeyValidationResult>.Fail(
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
            _logger.LogWarning(ex, "Redis cache unavailable while storing tenant context.");
        }

        return Result<ApiKeyValidationResult>.Ok(tenant);
    }
}