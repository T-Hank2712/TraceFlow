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
    private readonly TimeSpan _shutdownFlushTimeout;
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
        _flushInterval = TimeSpan.FromMilliseconds(options.Value.FlushIntervalMs);
        _shutdownFlushTimeout = TimeSpan.FromSeconds(options.Value.ShutdownFlushTimeoutSeconds);
    }

    protected override async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(
            _flushInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(
                cancellationToken))
            {
                try
                {
                    await FlushAndSignalCommitAsync(
                        cancellationToken);
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
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Shutdown path. Final flush runs in finally.
        }
        finally
        {
            await FlushRemainingBatchOnShutdownAsync();
        }
    }
    private async Task FlushAndSignalCommitAsync(
        CancellationToken cancellationToken)
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
    private async Task FlushRemainingBatchOnShutdownAsync()
    {
        using var timeout = new CancellationTokenSource(
            _shutdownFlushTimeout);

        try
        {
            _logger.LogInformation(
                "Flushing remaining log batch before shutdown.");

            await FlushAndSignalCommitAsync(
                timeout.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Shutdown batch flush timed out after {TimeoutSeconds} seconds.",
                _shutdownFlushTimeout.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to flush remaining log batch during shutdown.");
        }
    }
}
