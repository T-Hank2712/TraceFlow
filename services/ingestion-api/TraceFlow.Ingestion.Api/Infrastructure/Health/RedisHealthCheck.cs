namespace TraceFlow.Ingestion.Api.Infrastructure.Health;

public sealed class RedisHealthCheck(
    IConnectionMultiplexer redis)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = redis.GetDatabase();

            var latency = await database.PingAsync();

            return redis.IsConnected
                ? HealthCheckResult.Healthy(
                    $"Redis is connected. Latency: {latency.TotalMilliseconds} ms.")
                : HealthCheckResult.Unhealthy(
                    "Redis is not connected.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Redis is unavailable.",
                ex);
        }
    }
}