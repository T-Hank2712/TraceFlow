using TraceFlow.LogProcessor.Configurations;
using TraceFlow.LogProcessor.Services.Batching;
using Microsoft.Extensions.Options;

namespace TraceFlow.LogProcessor.Workers;

public sealed class BatchFlushWorker : BackgroundService
{
    private readonly IBatchProcessor _batchProcessor;
    private readonly ILogger<BatchFlushWorker> _logger;
    private readonly TimeSpan _flushInterval;
    public BatchFlushWorker(IBatchProcessor batchProcessor, ILogger<BatchFlushWorker> logger, IOptions<ProcessorOptions> options)
    {
        _batchProcessor = batchProcessor;
        _logger = logger;

        _flushInterval = TimeSpan.FromMilliseconds(
            options.Value.FlushIntervalMs);
    }
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(_flushInterval);

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                _logger.LogInformation(
                "Batch flush interval reached.");
                await _batchProcessor.FlushAsync(cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to flush log batch.");
            }
        }
    }
}
