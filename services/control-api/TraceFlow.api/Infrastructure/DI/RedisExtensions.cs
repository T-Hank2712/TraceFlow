namespace TraceFlow.Api.Infrastructure.DependencyInjection;

public static class RedisExtensions
{
    public static IServiceCollection AddApiKeyRevocationStore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString = configuration
            .GetSection(RedisOptions.SectionName)["ConnectionString"]
            ?? throw new InvalidOperationException(
                "Redis connection string is not configured.");

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton<IApiKeyRevocationStore, RedisApiKeyRevocationStore>();

        return services;
    }
}
