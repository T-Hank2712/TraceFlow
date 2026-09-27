using Testcontainers.Kafka;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

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
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}