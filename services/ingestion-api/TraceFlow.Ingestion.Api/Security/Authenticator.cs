using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TraceFlow.Ingestion.Api.Clients;
using TraceFlow.Ingestion.Api.Configurations;
using TraceFlow.Ingestion.Api.Contracts;
using TraceFlow.Ingestion.Api.Errors;
using TraceFlow.Ingestion.Api.Services.Ingestion;
using TraceFlow.Ingestion.Api.Services.RateLimiting;
using TraceFlow.Ingestion.Api.Services.Redis;

namespace TraceFlow.Ingestion.Api.Security;

public sealed class Authenticator
{
    private readonly ApiKeyHeaderParser _apiKeyParser;
    private readonly IApiKeyValidator _apiKeyValidator;
    private readonly IRedisCache _redisCache;
    private readonly RedisOptions _redisOptions;
    private readonly TenantContextCacheKey _tenantContextCacheKey;
    private readonly IRateLimiter _rateLimiter;
    private readonly ILogger _logger;

    public Authenticator(
        ApiKeyHeaderParser apiKeyParser,
        IApiKeyValidator apiKeyValidator,
        IRedisCache redisCache,
        IOptions<RedisOptions> redisOptions,
        TenantContextCacheKey tenantContextCacheKey,
        IRateLimiter rateLimiter,
        ILogger<Authenticator> logger)
    {
        _apiKeyParser = apiKeyParser;
        _apiKeyValidator = apiKeyValidator;
        _redisCache = redisCache;
        _redisOptions = redisOptions.Value;
        _tenantContextCacheKey = tenantContextCacheKey;
        _rateLimiter = rateLimiter;
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

        ApiKeyValidationResult? tenant = null;

        try
        {
            tenant = await _redisCache.GetAsync<ApiKeyValidationResult>(
                cacheKey,
                cancellationToken);

            if (tenant is not null)
            {
                _logger.LogInformation(
                    "Tenant context cache hit.");
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

            tenant = await _apiKeyValidator.ValidateAsync(
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
                _logger.LogWarning(
                    ex,
                    "Redis cache unavailable while storing tenant context.");
            }
        }

        var rateLimit = await _rateLimiter.CheckAsync(
            apiKey,
            cancellationToken);

        if (!rateLimit.Allowed)
        {
            return Result<ApiKeyValidationResult>.Fail(
                "rate_limit_exceeded",
                "Too many requests.",
                StatusCodes.Status429TooManyRequests);
        }

        return Result<ApiKeyValidationResult>.Ok(tenant);
    }
}