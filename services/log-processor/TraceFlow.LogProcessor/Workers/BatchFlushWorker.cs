using Microsoft.Extensions.Options;
using TraceFlow.LogProcessor.Configurations;
using TraceFlow.LogProcessor.Services.Batching;
using TraceFlow.LogProcessor.Services.Offsets;

namespace TraceFlow.LogProcessor.Workers;

public sealed class BatchFlushWorker : BackgroundService
{
    private readonly IBatchProcessor _batchProcessor;
    private readonly IOffsetCoordinator _offsetCoordinator;
    private readonly ILogger<BatchFlushWorker> _logger;
    private readonly TimeSpan _flushInterval;
    private readonly IOffsetCommitSignal _offsetCommitSignal;
    public BatchFlushWorker(
        IBatchProcessor batchProcessor,
        IOffsetCoordinator offsetCoordinator,
        ILogger<BatchFlushWorker> logger,
        IOptions<ProcessorOptions> options,
        IOffsetCommitSignal offsetCommitSignal)
    {
        _batchProcessor = batchProcessor;
        _offsetCoordinator = offsetCoordinator;
        _logger = logger;
        _offsetCommitSignal = offsetCommitSignal;

        _flushInterval = TimeSpan.FromMilliseconds(
            options.Value.FlushIntervalMs);
    }

    protected override async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(
            _flushInterval);

        while (await timer.WaitForNextTickAsync(
            cancellationToken))
        {
            try
            {
                var result = await _batchProcessor.FlushAsync(
                    cancellationToken);

                _offsetCoordinator.MarkProcessed(
                    result.ProcessedOffsets);

                if (result.ProcessedOffsets.Count > 0)
                {
                    _offsetCommitSignal.Signal();
                }
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
