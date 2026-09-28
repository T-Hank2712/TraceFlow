using System.Text.Json;
using Confluent.Kafka;

namespace TraceFlow.LogProcessor.IntegrationTests.Infrastructure;

public sealed class KafkaTestConsumer<T> : IDisposable
{
    private readonly IConsumer<Ignore, string> _consumer;

    public KafkaTestConsumer(
        string bootstrapServers,
        string topic)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = $"traceflow-log-processor-tests-{Guid.NewGuid()}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        _consumer.Subscribe(topic);
    }

    public T ConsumeUntil(
        Func<T, bool> predicate,
        TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);

        while (DateTimeOffset.UtcNow < deadline)
        {
            var result = _consumer.Consume(TimeSpan.FromMilliseconds(250));

            if (result is null)
            {
                continue;
            }

            var value = JsonSerializer.Deserialize<T>(
                result.Message.Value,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;

            if (predicate(value))
            {
                return value;
            }
        }

        throw new TimeoutException("No matching Kafka message was consumed.");
    }

    public void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
    }
}