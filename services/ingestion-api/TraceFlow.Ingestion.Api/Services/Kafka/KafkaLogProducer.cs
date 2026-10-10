namespace TraceFlow.Ingestion.Api.Kafka;

public sealed class KafkaLogProducer : ILogEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaLogProducer> _logger;
    public KafkaLogProducer(IOptions<KafkaOptions> options, ILogger<KafkaLogProducer> logger)
    {
        _options = options.Value;
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Acks.All,
            MessageSendMaxRetries = _options.MessageSendMaxRetries,
            RetryBackoffMs = _options.RetryBackoffMs,
            MessageTimeoutMs = _options.MessageTimeoutMs,
            QueueBufferingMaxMessages = _options.QueueBufferingMaxMessages,
            QueueBufferingMaxKbytes = _options.QueueBufferingMaxKbytes,
            LingerMs = _options.LingerMs
        };
        config.Set("delivery.timeout.ms", _options.DeliveryTimeoutMs.ToString());
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(
        EnrichedLogEvent logEvent,
        CancellationToken cancellationToken = default)
    {
        var eventId = logEvent.EventId;

        try
        {
            await _producer.ProduceAsync(
                _options.Topic,
                CreateMessage(logEvent),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error publishing log event. EventId: {EventId}",
                eventId);

            throw;
        }
    }

    public async Task<IReadOnlyList<BatchLogItemResult>> PublishAsync(
    IReadOnlyList<(int Index, EnrichedLogEvent Event)> logEvents, CancellationToken cancellationToken = default)
    {
        if (logEvents.Count == 0)
        {
            return Array.Empty<BatchLogItemResult>();
        }

        var concurrency = Math.Min(
            _options.BatchPublishConcurrency,
            logEvents.Count);

        using var inFlight = new SemaphoreSlim(concurrency);

        var tasks = logEvents.Select(item =>
            ProduceBatchItemAsync(
                item.Index,
                item.Event,
                inFlight,
                cancellationToken));

        return await Task.WhenAll(tasks);
    }
    private static Message<string, string> CreateMessage(
        EnrichedLogEvent logEvent)
    {
        return new Message<string, string>
        {
            Key = logEvent.EventId.ToString(),
            Value = JsonSerializer.Serialize(logEvent)
        };
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
    
    private async Task<BatchLogItemResult> ProduceBatchItemAsync(
        int index,
        EnrichedLogEvent logEvent,
        SemaphoreSlim inFlight,
        CancellationToken cancellationToken)
    {
        await inFlight.WaitAsync(cancellationToken);

        var eventId = logEvent.EventId;
        var released = 0;

        void ReleaseSlot()
        {
            if (Interlocked.Exchange(ref released, 1) == 0)
            {
                inFlight.Release();
            }
        }

        var completion =
            new TaskCompletionSource<BatchLogItemResult>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        using var cancellationRegistration = cancellationToken.Register(() =>
        {
            ReleaseSlot();
            completion.TrySetCanceled(cancellationToken);
        });

        try
        {
            _producer.Produce(
                _options.Topic,
                CreateMessage(logEvent),
                deliveryReport =>
                {
                    ReleaseSlot();

                    if (deliveryReport.Error.IsError)
                    {
                        _logger.LogError(
                            "Kafka delivery failed. EventId: {EventId}, Error: {Error}",
                            eventId,
                            deliveryReport.Error.Reason);

                        completion.TrySetResult(
                            new BatchLogItemResult(
                                Index: index,
                                Accepted: false,
                                EventId: eventId,
                                Error: "Failed to publish log event to Kafka."));

                        return;
                    }

                    completion.TrySetResult(
                        new BatchLogItemResult(
                            Index: index,
                            Accepted: true,
                            EventId: eventId,
                            Error: null));
                });
        }
        catch (ProduceException<string, string> ex)
        {
            ReleaseSlot();

            _logger.LogError(
                ex,
                "Kafka produce failed before delivery callback. EventId: {EventId}, Error: {Error}",
                eventId,
                ex.Error.Reason);

            completion.TrySetResult(
                new BatchLogItemResult(
                    Index: index,
                    Accepted: false,
                    EventId: eventId,
                    Error: "Failed to publish log event to Kafka."));
        }
        catch (Exception ex)
        {
            ReleaseSlot();

            _logger.LogError(
                ex,
                "Unexpected error enqueueing Kafka message. EventId: {EventId}",
                eventId);

            completion.TrySetResult(
                new BatchLogItemResult(
                    Index: index,
                    Accepted: false,
                    EventId: eventId,
                    Error: "Kafka publish failed."));
        }

        return await completion.Task;
    }
}
