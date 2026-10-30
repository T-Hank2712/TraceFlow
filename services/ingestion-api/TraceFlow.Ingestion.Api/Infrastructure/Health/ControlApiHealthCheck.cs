namespace TraceFlow.Ingestion.Api.Infrastructure.Health;

public sealed class ControlApiHealthCheck(
    IHttpClientFactory httpClientFactory,
    IOptions<ControlApiOptions> options)
    : IHealthCheck
{
    private const string ReadinessProbeApiKey =
        "tfk_live_01ARZ3NDEKTSV4RRFFQ69G5FAV_readiness_probe";

    private readonly ControlApiOptions _options = options.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient("control-api-health");

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                _options.ValidateApiPath);

            request.Headers.Add(
                "X-Internal-Secret",
                _options.InternalServiceSecret);
            request.Content = JsonContent.Create(new
            {
                apiKey = ReadinessProbeApiKey
            });

            using var response = await client.SendAsync(
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy(
                    $"Control API validation returned {(int)response.StatusCode}.");
            }

            var validationResult = await response.Content.ReadFromJsonAsync<ApiKeyValidationResult>(
                cancellationToken);

            return validationResult is not null
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy(
                    "Control API validation returned an empty response.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Control API is unavailable.",
                ex);
        }
    }
}
