using Microsoft.Extensions.Options;
using TraceFlow.LogProcessor.Configurations;
using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.Services.OpenSearch;

namespace TraceFlow.LogProcessor.Services.Batching;

public sealed class BatchProcessor : IBatchProcessor
{
    private readonly ProcessorOptions _options;
    private readonly ILogger<BatchProcessor> _logger;
    private readonly List<LogEvent> _buffer = [];
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ILogIndexer _logIndexer;
    public BatchProcessor(IOptions<ProcessorOptions> options, ILogger<BatchProcessor> logger, ILogIndexer logIndexer)
    {
        _options = options.Value;
        _logger = logger;
        _logIndexer = logIndexer;
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
    public async Task FlushInternalAsync(CancellationToken cancellationToken)
    {
        if (_buffer.Count == 0) return;

        var batch = _buffer.ToList();

        _buffer.Clear();

        _logger.LogInformation("Flushing log batch. Count: {Count}", batch.Count);

        await _logIndexer.IndexAsync(batch, cancellationToken);
    }
}