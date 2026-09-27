using System.Text.Json;
using Confluent.Kafka;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

public sealed class KafkaTestConsumer<T> : IDisposable
{
    private readonly IConsumer<string, string> _consumer;

    public KafkaTestConsumer(string bootstrapServers, string topic)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = $"traceflow-tests-{Guid.NewGuid()}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe(topic);
    }

    public T ConsumeOne(TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);

        while (DateTimeOffset.UtcNow < deadline)
        {
            var result = _consumer.Consume(TimeSpan.FromMilliseconds(250));

            if (result is null)
            {
                continue;
            }

            return JsonSerializer.Deserialize<T>(
                result.Message.Value,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
        }

        throw new TimeoutException("No Kafka message was consumed.");
    }

    public void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
    }
}