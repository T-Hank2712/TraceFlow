namespace TraceFlow.Ingestion.Api.Infrastructure.Health;

public sealed class ControlApiHealthCheck(
    IHttpClientFactory httpClientFactory)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient("control-api-health");

            using var response = await client.GetAsync(
                "/health",
                cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy(
                    $"Control API returned {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Control API is unavailable.",
                ex);
        }
    }
}