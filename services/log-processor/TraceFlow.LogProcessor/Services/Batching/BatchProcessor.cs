using Microsoft.Extensions.Options;
using TraceFlow.LogProcessor.Configurations;
using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.Services.Kafka;
using TraceFlow.LogProcessor.Services.OpenSearch;

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
        var events = batch
            .Select(x => x.Event)
            .ToList();

        var result = await _logIndexer.IndexAsync(
            events,
            cancellationToken);

        var succeededEventIds = result.SucceededEvents
            .Select(x => x.EventId)
            .ToHashSet();

        var failedEvents = result.FailedEvents
            .ToList();

        var dlqSucceededEventIds = new HashSet<string>();

        if (failedEvents.Count > 0)
        {
            _logger.LogInformation(
                "OpenSearch partial indexing failure. " +
                "Publishing {Count}/{Total} failed events to DLQ.",
                failedEvents.Count,
                batch.Count);

            var dlqEvents = failedEvents
                .Select(logEvent =>
                    new DlqLogEvent(
                        logEvent,
                        "OpenSearchItemFailure",
                        "OpenSearch failed to index the event.",
                        DateTimeOffset.UtcNow))
                .ToList();

            await _dlqProducer.PublishAsync(
                dlqEvents,
                cancellationToken);

            foreach (var failedEvent in failedEvents)
            {
                dlqSucceededEventIds.Add(
                    failedEvent.EventId);
            }
        }

        var processedOffsets = batch
            .Where(x =>
                succeededEventIds.Contains(x.Event.EventId) ||
                dlqSucceededEventIds.Contains(x.Event.EventId))
            .Select(x => x.Offset)
            .ToList();

        return new BatchProcessResult(
            processedOffsets);
    }
    private List<PendingLogEvent> DrainBuffer()
    {
        var batch = _buffer.ToList();
        _buffer.Clear();

        return batch;
    }
}
