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

            var metadata = adminClient.GetMetadata(
                _options.Topic,
                TimeSpan.FromSeconds(_options.HealthCheckTimeoutSeconds));

            var topic = metadata.Topics.FirstOrDefault(topic =>
                string.Equals(topic.Topic, _options.Topic, StringComparison.Ordinal));

            if (topic is null)
            {
                return Task.FromResult(
                    HealthCheckResult.Unhealthy(
                        $"Kafka topic '{_options.Topic}' was not found."));
            }

            if (topic.Error.IsError)
            {
                return Task.FromResult(
                    HealthCheckResult.Unhealthy(
                        $"Kafka topic '{_options.Topic}' is unavailable: {topic.Error.Reason}."));
            }

            if (topic.Partitions.Count == 0)
            {
                return Task.FromResult(
                    HealthCheckResult.Unhealthy(
                        $"Kafka topic '{_options.Topic}' has no partitions."));
            }

            var unavailablePartitions = topic.Partitions
                .Where(partition => partition.Error.IsError)
                .Select(partition => partition.PartitionId.ToString())
                .ToArray();

            if (unavailablePartitions.Length > 0)
            {
                return Task.FromResult(
                    HealthCheckResult.Unhealthy(
                        $"Kafka topic '{_options.Topic}' has unavailable partitions: " +
                        string.Join(", ", unavailablePartitions) + "."));
            }

            return Task.FromResult(
                HealthCheckResult.Healthy(
                    $"Kafka topic '{_options.Topic}' is usable."));
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
