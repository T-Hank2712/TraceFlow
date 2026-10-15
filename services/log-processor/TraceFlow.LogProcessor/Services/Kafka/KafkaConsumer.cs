namespace TraceFlow.LogProcessor.Services.Kafka;

public sealed class KafkaConsumer : IKafkaConsumer, IDisposable
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaConsumer> _logger;
    private readonly ILogEventNormalizer _logEventNormalizer;
    private readonly IOffsetCoordinator _offsetCoordinator;
    private readonly IOffsetCommitSignal _offsetCommitSignal;
    private readonly IDlqProducer _dlqProducer;
    private readonly IPartitionLaneManager _laneManager;

    public KafkaConsumer(
        IOptions<KafkaOptions> options,
        ILogger<KafkaConsumer> logger,
        ILogEventNormalizer logEventNormalizer,
        IOffsetCoordinator offsetCoordinator,
        IOffsetCommitSignal offsetCommitSignal,
        IDlqProducer dlqProducer,
        IPartitionLaneManager laneManager)
    {
        _options = options.Value;
        _logger = logger;
        _logEventNormalizer = logEventNormalizer;
        _offsetCoordinator = offsetCoordinator;
        _offsetCommitSignal = offsetCommitSignal;
        _dlqProducer = dlqProducer;
        _laneManager = laneManager;

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            QueuedMinMessages = _options.QueuedMinMessages,
            QueuedMaxMessagesKbytes = _options.QueuedMaxMessagesKbytes,
            FetchMinBytes = _options.FetchMinBytes,
            FetchWaitMaxMs = _options.FetchWaitMaxMs,
            MaxPartitionFetchBytes = _options.MaxPartitionFetchBytes
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config)
            .Build();

        _consumer.Subscribe(_options.Topic);
    }

    public async Task ConsumeAsync(
        Func<PendingLogEvent, CancellationToken, Task<BatchProcessResult>> handler,
        CancellationToken cancellationToken)
    {

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
            try
            {
                await _laneManager.StopAsync(
                    CancellationToken.None);

                if (_offsetCommitSignal.IsSignaled())
                {
                    CommitAvailableOffsets();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to stop partition lanes cleanly.");
            }
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

        var messageValue = result.Message?.Value;
        if (string.IsNullOrWhiteSpace(messageValue))
        {
            _logger.LogWarning(
                "Received empty Kafka message. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                result.Topic,
                result.Partition,
                result.Offset);

            await PublishPoisonMessageToDlqAsync(
                result,
                "EmptyKafkaMessage",
                "Kafka message value is empty.",
                null,
                cancellationToken);

            return;
        }

        LogEvent? logEvent;

        try
        {
            logEvent = JsonSerializer.Deserialize<LogEvent>(
                messageValue);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to deserialize Kafka message. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                result.Topic,
                result.Partition,
                result.Offset);

            await PublishPoisonMessageToDlqAsync(
                result,
                "InvalidJson",
                "Kafka message cannot be deserialized into LogEvent.",
                ex,
                cancellationToken);

            return;
        }

        if (logEvent is null)
        {
            _logger.LogWarning(
                "Kafka message deserialized to null. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                result.Topic,
                result.Partition,
                result.Offset);

            await PublishPoisonMessageToDlqAsync(
                result,
                "NullLogEvent",
                "Kafka message deserialized to null LogEvent.",
                null,
                cancellationToken);

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

            await PublishPoisonMessageToDlqAsync(
                result,
                "InvalidLogEventFormat",
                "Kafka message contains invalid log event data.",
                ex,
                cancellationToken);

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

        if (processResult.ProcessedOffsets.Count > 0)
        {
            _offsetCommitSignal.Signal();
        }
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
    }

    public void Dispose()
    {
        _consumer.Dispose();
    }
    private async Task PublishPoisonMessageToDlqAsync(
        ConsumeResult<Ignore, string> result,
        string failureType,
        string failureReason,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        var rawPayload = result.Message?.Value;

        var dlqEvent = new DlqLogEvent(
            null,
            rawPayload,
            failureType,
            exception is null
                ? failureReason
                : $"{failureReason} {exception.Message}",
            result.Topic,
            result.Partition.Value,
            result.Offset.Value,
            DateTimeOffset.UtcNow);

        try
        {
            await _dlqProducer.PublishAsync(
                [dlqEvent],
                cancellationToken);

            _offsetCoordinator.MarkProcessed(
                [result.TopicPartitionOffset]);

            _offsetCommitSignal.Signal();
        }
        catch (Exception dlqException)
            when (dlqException is not OperationCanceledException)
        {
            _logger.LogError(
                dlqException,
                "Failed to publish poison Kafka message to DLQ. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, FailureType: {FailureType}",
                result.Topic,
                result.Partition,
                result.Offset,
                failureType);
        }
    }
}
