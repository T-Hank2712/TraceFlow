using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using TraceFlow.LogProcessor.Configurations;
using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.Processing;

namespace TraceFlow.LogProcessor.Services.Kafka;

public sealed class KafkaConsumer : IKafkaConsumer, IDisposable
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaConsumer> _logger;
    private readonly ILogEventNormalizer _logEventNormalizer;
    public KafkaConsumer(IOptions<KafkaOptions> options, ILogger<KafkaConsumer> logger, ILogEventNormalizer logEventNormalizer)
    {
        _options = options.Value;
        _logger = logger;
        _logEventNormalizer = logEventNormalizer;
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };
        _consumer = new ConsumerBuilder<Ignore, string>(config)
            .Build();

        _consumer.Subscribe(_options.Topic);
    }
    public async Task ConsumeAsync(
        Func<LogEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Kafka consumer started. Topic: {Topic}, GroupId: {GroupId}", _options.Topic, _options.GroupId);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string> result;
                try
                {
                    result = _consumer.Consume(cancellationToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consumption error. Error: {Reason}", ex.Error.Reason);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(result.Message.Value))
                {
                    _logger.LogWarning(
                        "Received empty Kafka message. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                        result.Topic,
                        result.Partition,
                        result.Offset);
                    continue;
                }
                LogEvent? logEvent;
                
                try
                {
                    logEvent = JsonSerializer.Deserialize<LogEvent>(result.Message.Value);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to deserialize Kafka message. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                        result.Topic,
                        result.Partition,
                        result.Offset);

                    continue;
                }

                if (logEvent is null)
                {
                    _logger.LogWarning(
                        "Kafka message deserialized to null. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                        result.Topic,
                        result.Partition,
                        result.Offset);

                    continue;
                }

                try
                {
                    logEvent = _logEventNormalizer.Normalize(logEvent);
                }
                catch (FormatException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Incorrect data format, Offset: {Offset}",
                        result.Offset);

                    continue;
                }

                await handler(logEvent, cancellationToken);
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Kafka consumer cancellation requested.");
        }
        finally
        {
            _consumer.Close();

            _logger.LogInformation("Kafka consumer stopped.");
        }

    }
    public void Dispose()
    {
        _consumer.Dispose();
    }
}
