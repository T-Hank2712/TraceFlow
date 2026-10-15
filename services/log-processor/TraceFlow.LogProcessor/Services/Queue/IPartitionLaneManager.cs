namespace TraceFlow.LogProcessor.Services.Queue;

public interface IPartitionLaneManager
{
    ValueTask EnqueueAsync(
        PendingLogEvent pendingEvent,
        CancellationToken cancellationToken);

    Task StopAsync(
        CancellationToken cancellationToken);
}