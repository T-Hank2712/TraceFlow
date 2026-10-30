namespace TraceFlow.Ingestion.Api.Infrastructure.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddIngestionHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var controlApiOptions = configuration
            .GetSection("ControlApi")
            .Get<ControlApiOptions>()
            ?? throw new InvalidOperationException(
                "Control API options are not configured.");

        services.AddHttpClient(
            "control-api-health",
            client =>
            {
                client.BaseAddress = new Uri(controlApiOptions.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(controlApiOptions.TimeoutSeconds);
            });

        services
            .AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy(),
                tags: ["live"])
            .AddCheck<KafkaHealthCheck>(
                "kafka",
                tags: ["ready"])
            .AddCheck<ControlApiHealthCheck>(
                "control-api",
                tags: ["ready"])
            .AddCheck<RedisHealthCheck>(
                "redis",
                tags: ["ready"]);

        return services;
    }
}
