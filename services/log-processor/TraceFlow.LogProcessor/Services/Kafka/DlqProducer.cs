using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using TraceFlow.LogProcessor.Configurations;
using TraceFlow.LogProcessor.Contracts;

namespace TraceFlow.LogProcessor.Services.Kafka;

public sealed class DlqProducer : IDlqProducer, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly KafkaOptions _kafkaOptions;
    private readonly ILogger<DlqProducer> _logger;
    public DlqProducer(IOptions<KafkaOptions> kafkaOptions, ILogger<DlqProducer> logger)
    {
        _kafkaOptions = kafkaOptions.Value;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            Acks = Acks.All
        };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }
    public async Task PublishAsync(IReadOnlyCollection<DlqLogEvent> events, CancellationToken cancellationToken)
    {
        foreach (var dlqEvent in events)
        {
            var value = JsonSerializer.Serialize(dlqEvent);
            await _producer.ProduceAsync(_kafkaOptions.DlqTopic, new Message<Null, string> { Value = value }, cancellationToken);
        }
        _logger.LogInformation("Published failed log events to DLQ. Count: {Count}, Topic: {Topic}",
        events.Count, _kafkaOptions.DlqTopic);
    }
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
