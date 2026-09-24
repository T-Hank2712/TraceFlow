using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using TraceFlow.LogProcessor.Configurations;
using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.Processing;
using TraceFlow.LogProcessor.Services.Batching;
using TraceFlow.LogProcessor.Services.Offsets;

namespace TraceFlow.LogProcessor.Services.Kafka;

public sealed class KafkaConsumer : IKafkaConsumer, IDisposable
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaConsumer> _logger;
    private readonly ILogEventNormalizer _logEventNormalizer;
    private readonly IOffsetCoordinator _offsetCoordinator;
    private readonly IOffsetCommitSignal _offsetCommitSignal;

    public KafkaConsumer(
        IOptions<KafkaOptions> options,
        ILogger<KafkaConsumer> logger,
        ILogEventNormalizer logEventNormalizer,
        IOffsetCoordinator offsetCoordinator,
        IOffsetCommitSignal offsetCommitSignal)
    {
        _options = options.Value;
        _logger = logger;
        _logEventNormalizer = logEventNormalizer;
        _offsetCoordinator = offsetCoordinator;
        _offsetCommitSignal = offsetCommitSignal;

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config)
            .Build();

        _consumer.Subscribe(_options.Topic);
    }

    public async Task ConsumeAsync(
        Func<PendingLogEvent, CancellationToken, Task<BatchProcessResult>> handler,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Kafka consumer started. Topic: {Topic}, GroupId: {GroupId}",
            _options.Topic,
            _options.GroupId);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string>? result;

                try
                {
                    result = _consumer.Consume(
                        TimeSpan.FromMilliseconds(
                            _options.PollTimeoutMs));
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(
                        ex,
                        "Kafka consumption error. Error: {Reason}",
                        ex.Error.Reason);

                    continue;
                }

                if (result is not null)
                {
                    await ProcessMessageAsync(
                        result,
                        handler,
                        cancellationToken);
                }
                if (_offsetCommitSignal.IsSignaled())
                {
                    CommitAvailableOffsets();
                }
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Kafka consumer cancellation requested.");
        }
        finally
        {
            _consumer.Close();

            _logger.LogInformation(
                "Kafka consumer stopped.");
        }
    }

    private async Task ProcessMessageAsync(
        ConsumeResult<Ignore, string> result,
        Func<PendingLogEvent, CancellationToken, Task<BatchProcessResult>> handler,
        CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(result.Message.Value))
        {
            _logger.LogWarning(
                "Received empty Kafka message. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                result.Topic,
                result.Partition,
                result.Offset);

            return;
        }

        LogEvent? logEvent;

        try
        {
            logEvent = JsonSerializer.Deserialize<LogEvent>(
                result.Message.Value);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to deserialize Kafka message. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                result.Topic,
                result.Partition,
                result.Offset);

            return;
        }

        if (logEvent is null)
        {
            _logger.LogWarning(
                "Kafka message deserialized to null. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                result.Topic,
                result.Partition,
                result.Offset);

            return;
        }

        try
        {
            logEvent = _logEventNormalizer.Normalize(logEvent);
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(
                ex,
                "Incorrect data format. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                result.Topic,
                result.Partition,
                result.Offset);

            return;
        }

        var pendingEvent = new PendingLogEvent(
            logEvent,
            result.TopicPartitionOffset);

        var processResult = await handler(
            pendingEvent,
            cancellationToken);

        _offsetCoordinator.MarkProcessed(
            processResult.ProcessedOffsets);
    }

    private void CommitAvailableOffsets()
    {

        var offsets = _offsetCoordinator
            .GetCommittableOffsets();

        if (offsets.Count == 0)
        {
            return;
        }

        _consumer.Commit(offsets);

        _logger.LogInformation(
            "Committed Kafka offsets. Count: {Count}",
            offsets.Count);
    }

    public void Dispose()
    {
        _consumer.Dispose();
    }
}
