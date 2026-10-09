namespace TraceFlow.Ingestion.Api.Infrastructure.Extensions;

public static class RedisExtensions
{
    public static IServiceCollection AddRedisInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString =
            configuration.GetSection(RedisOptions.SectionName)["ConnectionString"]
            ?? throw new InvalidOperationException(
                "Redis connection string is not configured.");

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddSingleton<IRedisCache, RedisCache>();
        services.AddSingleton<IRateLimiter, RedisRateLimiter>();
        services.AddSingleton<TenantContextCacheKey>();
        services.AddMemoryCache();

        return services;
    }
}