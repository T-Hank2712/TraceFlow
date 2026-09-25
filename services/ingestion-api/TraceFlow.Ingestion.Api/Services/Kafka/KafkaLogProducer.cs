using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using TraceFlow.Ingestion.Api.Configuration;
using TraceFlow.Ingestion.Api.Services.Ingestion;
using TraceFlow.Ingestion.Api.Contracts.Log;

namespace TraceFlow.Ingestion.Api.Kafka;

public sealed class KafkaLogProducer : ILogEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _options;
    public KafkaLogProducer(IOptions<KafkaOptions> options)
    {
        _options = options.Value;
        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            MessageTimeoutMs = _options.DeliveryTimeoutSeconds * 1000,
            Acks = Acks.All
        }).Build();
    }

    public async Task PublishAsync(EnrichedLogEvent logEvent, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(logEvent);

        await _producer.ProduceAsync(
            _options.Topic,
            new Message<string, string>
            {
                Key = logEvent.ProjectId.ToString(),
                Value = payload
            },
            cancellationToken
        );
    }
    public async Task PublishAsync(IReadOnlyList<EnrichedLogEvent> logEvents, CancellationToken cancellationToken)
    {
        foreach (var logEvent in logEvents)
        {
            var payload = JsonSerializer.Serialize(logEvent);

            await _producer.ProduceAsync(
                _options.Topic,
                new Message<string, string>
                {
                    Key = logEvent.ProjectId.ToString(),
                    Value = payload
                },
                cancellationToken);
        }
    }
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
