using TraceFlow.LogProcessor.Services.Batching;
using TraceFlow.LogProcessor.Services.Offsets;
using TraceFlow.LogProcessor.Services.Queue;

namespace TraceFlow.LogProcessor.Workers;

public sealed class QueuedBatchWorker : BackgroundService
{
    private readonly IPendingLogEventQueue _queue;
    private readonly IBatchProcessor _batchProcessor;
    private readonly IOffsetCoordinator _offsetCoordinator;
    private readonly IOffsetCommitSignal _offsetCommitSignal;
    private readonly ILogger<QueuedBatchWorker> _logger;

    public QueuedBatchWorker(
        IPendingLogEventQueue queue,
        IBatchProcessor batchProcessor,
        IOffsetCoordinator offsetCoordinator,
        IOffsetCommitSignal offsetCommitSignal,
        ILogger<QueuedBatchWorker> logger)
    {
        _queue = queue;
        _batchProcessor = batchProcessor;
        _offsetCoordinator = offsetCoordinator;
        _offsetCommitSignal = offsetCommitSignal;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued batch worker started.");

        try
        {
            await foreach (var pendingEvent in _queue.ReadAllAsync(stoppingToken))
            {
                var result = await _batchProcessor.AddAsync(
                    pendingEvent,
                    stoppingToken);

                _offsetCoordinator.MarkProcessed(
                    result.ProcessedOffsets);

                if (result.ProcessedOffsets.Count > 0)
                {
                    _offsetCommitSignal.Signal();
                }
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Queued batch worker cancellation requested.");
        }

        _logger.LogInformation("Queued batch worker stopped.");
    }
}
