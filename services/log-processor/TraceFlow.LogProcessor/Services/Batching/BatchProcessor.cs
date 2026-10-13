namespace TraceFlow.LogProcessor.Services.Batching;

public sealed class BatchProcessor : IBatchProcessor
{
    private readonly ProcessorOptions _options;
    private readonly ILogger<BatchProcessor> _logger;
    private readonly List<PendingLogEvent> _buffer = [];
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ILogIndexer _logIndexer;
    private readonly IDlqProducer _dlqProducer;
    public BatchProcessor(IOptions<ProcessorOptions> options, ILogger<BatchProcessor> logger, ILogIndexer logIndexer, IDlqProducer dlqProducer)
    {
        _options = options.Value;
        _logger = logger;
        _logIndexer = logIndexer;
        _dlqProducer = dlqProducer;
    }
    public async Task<BatchProcessResult> AddAsync(
        PendingLogEvent pendingEvent,
        CancellationToken cancellationToken)
    {
        List<PendingLogEvent>? batch = null;

        await _lock.WaitAsync(cancellationToken);

        try
        {
            _buffer.Add(pendingEvent);

            if (_buffer.Count >= _options.BatchSize)
            {
                batch = DrainBuffer();
            }
        }
        finally
        {
            _lock.Release();
        }

        if (batch is null)
        {
            return new BatchProcessResult([]);
        }

        return await ProcessBatchAsync(
            batch,
            cancellationToken);
    }
    public async Task<BatchProcessResult> FlushAsync(
     CancellationToken cancellationToken)
    {
        List<PendingLogEvent>? batch = null;

        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (_buffer.Count > 0)
            {
                batch = DrainBuffer();
            }
        }
        finally
        {
            _lock.Release();
        }

        if (batch is null)
        {
            return new BatchProcessResult([]);
        }

        return await ProcessBatchAsync(
            batch,
            cancellationToken);
    }
    private async Task<BatchProcessResult> ProcessBatchAsync(
        IReadOnlyList<PendingLogEvent> batch,
        CancellationToken cancellationToken)
    {
        var events = CreateLogEvents(batch);

        BulkIndexResult result;

        try
        {
            result = await _logIndexer.IndexAsync(
                events,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(
                ex,
                "OpenSearch full batch indexing failure. Publishing {Count} events to DLQ.",
                batch.Count);

            return await PublishFailedBatchToDlqAsync(
                batch,
                cancellationToken);
        }

        return await BuildIndexResultAsync(
            batch,
            result,
            cancellationToken);
    }

    private List<PendingLogEvent> DrainBuffer()
    {
        var batch = new List<PendingLogEvent>(_buffer.Count);
        batch.AddRange(_buffer);
        _buffer.Clear();

        return batch;
    }
    private static List<LogEvent> CreateLogEvents(
        IReadOnlyList<PendingLogEvent> batch)
    {
        var events = new List<LogEvent>(batch.Count);

        for (var index = 0; index < batch.Count; index++)
        {
            events.Add(batch[index].Event);
        }

        return events;
    }
    private static DlqLogEvent CreateDlqEvent(
        PendingLogEvent pendingEvent,
        string failureType,
        string failureReason)
    {
        return new DlqLogEvent(
            pendingEvent.Event,
            null,
            failureType,
            failureReason,
            pendingEvent.Offset.Topic,
            pendingEvent.Offset.Partition.Value,
            pendingEvent.Offset.Offset.Value,
            DateTimeOffset.UtcNow);
    }
    private async Task<BatchProcessResult> PublishFailedBatchToDlqAsync(
        IReadOnlyList<PendingLogEvent> batch,
        CancellationToken cancellationToken)
    {
        var dlqEvents = new List<DlqLogEvent>(batch.Count);
        var processedOffsets = new List<TopicPartitionOffset>(batch.Count);

        for (var index = 0; index < batch.Count; index++)
        {
            var pendingEvent = batch[index];

            dlqEvents.Add(
                CreateDlqEvent(
                    pendingEvent,
                    "OpenSearchBatchFailure",
                    "OpenSearch failed to index the full batch."));

            processedOffsets.Add(
                pendingEvent.Offset);
        }

        await _dlqProducer.PublishAsync(
            dlqEvents,
            cancellationToken);

        return new BatchProcessResult(
            processedOffsets);
    }
    private async Task<BatchProcessResult> BuildIndexResultAsync(
        IReadOnlyList<PendingLogEvent> batch,
        BulkIndexResult result,
        CancellationToken cancellationToken)
    {
        if (result.FailedEvents.Count > 0)
        {
            _logger.LogInformation(
                "OpenSearch partial indexing failure. " +
                "Publishing {Count}/{Total} failed events to DLQ.",
                result.FailedEvents.Count,
                batch.Count);

            var pendingEventByEventId = new Dictionary<string, PendingLogEvent>(
                batch.Count);

            for (var index = 0; index < batch.Count; index++)
            {
                var pendingEvent = batch[index];

                pendingEventByEventId[pendingEvent.Event.EventId] = pendingEvent;
            }

            var dlqEvents = new List<DlqLogEvent>(
                result.FailedEvents.Count);

            foreach (var failedEvent in result.FailedEvents)
            {
                var pendingEvent = pendingEventByEventId[failedEvent.EventId];

                dlqEvents.Add(
                    CreateDlqEvent(
                        pendingEvent,
                        "OpenSearchItemFailure",
                        "OpenSearch failed to index the event."));
            }

            await _dlqProducer.PublishAsync(
                dlqEvents,
                cancellationToken);
        }

        var processedOffsets = new List<TopicPartitionOffset>(
            batch.Count);

        for (var index = 0; index < batch.Count; index++)
        {
            processedOffsets.Add(
                batch[index].Offset);
        }

        return new BatchProcessResult(
            processedOffsets);
    }
}
