using Microsoft.Extensions.Options;
using TraceFlow.LogProcessor.Configurations;
using TraceFlow.LogProcessor.Contracts;

namespace TraceFlow.LogProcessor.Services.Batching;

public sealed class BatchProcessor : IBatchProcessor
{
    private readonly ProcessorOptions _options;
    private readonly ILogger<BatchProcessor> _logger;
    private readonly List<LogEvent> _buffer = [];
    private readonly SemaphoreSlim _lock = new(1, 1);
    public BatchProcessor(IOptions<ProcessorOptions> options, ILogger<BatchProcessor> logger)
    {
        _options = options.Value;
        _logger = logger;
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
    public Task FlushInternalAsync(CancellationToken cancellationToken)
    {
        if (_buffer.Count == 0) return Task.CompletedTask;

        var batch = _buffer.ToList();

        _buffer.Clear();

        _logger.LogInformation("Flushing log batch. Count: {Count}", batch.Count);

        return Task.CompletedTask;
    }
}