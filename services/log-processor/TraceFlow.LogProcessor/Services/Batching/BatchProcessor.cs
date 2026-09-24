using Microsoft.Extensions.Options;
using TraceFlow.LogProcessor.Configurations;
using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.Services.Kafka;
using TraceFlow.LogProcessor.Services.OpenSearch;
using TraceFlow.LogProcessor.Extentions.Exceptions;

namespace TraceFlow.LogProcessor.Services.Batching;

public sealed class BatchProcessor : IBatchProcessor
{
    private readonly ProcessorOptions _options;
    private readonly ILogger<BatchProcessor> _logger;
    private readonly List<LogEvent> _buffer = [];
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
    public async Task AddAsync(LogEvent logEvent, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            _buffer.Add(logEvent);

            _logger.LogInformation("Add LogEvent into Batch: {EventId}", logEvent.EventId);

            if(_buffer.Count >= _options.BatchSize)
            {
                await FlushInternalAsync(cancellationToken);
            }
        }
        finally
        {
            _lock.Release();
        }
    }
    public async Task FlushAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            await FlushInternalAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }
    private async Task FlushInternalAsync(CancellationToken cancellationToken)
    {
        if (_buffer.Count == 0) return;

        var batchCount = _buffer.Count;

        _logger.LogInformation("Flushing log batch. Count: {Count}", batchCount);

        try
        {
            var result = await _logIndexer.IndexAsync(_buffer, cancellationToken);

            if(result.FailedEvents.Count > 0)
            {
                _logger.LogWarning(
                "OpenSearch partial indexing failure. Publishing {FailedCount}/{TotalCount} poison events to DLQ.",
                result.FailedEvents.Count,
                batchCount);

                var poisonDlqEvents = result.FailedEvents
                .Select(logEvent => new DlqLogEvent(
                    logEvent,
                    "OpenSearchBadDataFailure",
                    "Invalid document format or mapping error in OpenSearch",
                    DateTimeOffset.UtcNow))
                .ToList();

                await _dlqProducer.PublishAsync(poisonDlqEvents, cancellationToken);
            }

            _buffer.Clear();

            _logger.LogInformation(
                "Log batch processed successfully. Indexed: {SucceededCount}, Sent to DLQ: {FailedCount}",
                result.SucceededEvents.Count,
                result.FailedEvents.Count);
        }
        catch (OpenSearchBulkException ex)
        {
            _logger.LogError(ex, "OpenSearch bulk indexing failed after retries. Publishing batch to DLQ. Count: {Count}", _buffer.Count);

            var dlqEvents = _buffer
                .Select(logEvent =>
                    new DlqLogEvent(
                        logEvent,
                        "OpenSearchBulkFailure",
                        ex.Message,
                        DateTimeOffset.UtcNow))
                .ToList();

            try
            {
                await _dlqProducer.PublishAsync(dlqEvents, cancellationToken);

                _buffer.RemoveRange(0, batchCount);

                _logger.LogInformation("Log batch moved to DLQ successfully. Count: {Count}", batchCount);
            }
            catch (Exception dlqException) when (dlqException is not OperationCanceledException)
            {
                _logger.LogError(
                    dlqException,
                    "Failed to publish log batch to DLQ. Batch remains buffered. Count: {Count}",
                    batchCount);
                    
                throw;
            }
        }
    }
}