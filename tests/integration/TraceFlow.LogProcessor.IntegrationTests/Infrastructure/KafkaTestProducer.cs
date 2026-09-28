using System.Text.Json;
using Confluent.Kafka;
using TraceFlow.LogProcessor.Contracts;

namespace TraceFlow.LogProcessor.IntegrationTests.Infrastructure;

public sealed class KafkaTestProducer : IDisposable
{
    private readonly IProducer<Null, string> _producer;

    public KafkaTestProducer(string bootstrapServers)
    {
        _producer = new ProducerBuilder<Null, string>(
            new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All
            })
            .Build();
    }

    public async Task ProduceLogEventAsync(
        string topic,
        LogEvent logEvent,
        CancellationToken cancellationToken = default)
    {
        await _producer.ProduceAsync(
            topic,
            new Message<Null, string>
            {
                Value = JsonSerializer.Serialize(logEvent)
            },
            cancellationToken);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}