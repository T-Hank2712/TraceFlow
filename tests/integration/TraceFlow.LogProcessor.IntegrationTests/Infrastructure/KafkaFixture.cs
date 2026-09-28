using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Testcontainers.Kafka;

namespace TraceFlow.LogProcessor.IntegrationTests.Infrastructure;

public sealed class KafkaFixture : IAsyncLifetime
{
    private readonly KafkaContainer _container =
        new KafkaBuilder("apache/kafka:4.1.2")
            .Build();

    public string BootstrapServers =>
        _container.GetBootstrapAddress();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await CreateTopicAsync("traceflow.logs");
        await CreateTopicAsync("traceflow.logs.dlq");
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public async Task CreateTopicAsync(string topic)
    {
        using var adminClient = new AdminClientBuilder(
            new AdminClientConfig
            {
                BootstrapServers = BootstrapServers
            })
            .Build();

        try
        {
            await adminClient.CreateTopicsAsync(
            [
                new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
            ]);
        }
        catch (CreateTopicsException ex)
            when (ex.Results.Any(result =>
                result.Error.Code == ErrorCode.TopicAlreadyExists))
        {
        }
    }
}