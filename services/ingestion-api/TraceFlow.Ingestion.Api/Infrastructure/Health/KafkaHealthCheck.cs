namespace TraceFlow.Ingestion.Api.Infrastructure.Health;

public sealed class KafkaHealthCheck(
    IOptions<KafkaOptions> options)
    : IHealthCheck
{
    private readonly KafkaOptions _options = options.Value;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var adminClient = new AdminClientBuilder(
                new AdminClientConfig
                {
                    BootstrapServers = _options.BootstrapServers
                })
                .Build();

            adminClient.GetMetadata(
                _options.Topic,
                TimeSpan.FromSeconds(3));

            return Task.FromResult(
                HealthCheckResult.Healthy());
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "Kafka is unavailable.",
                    ex));
        }
    }
}