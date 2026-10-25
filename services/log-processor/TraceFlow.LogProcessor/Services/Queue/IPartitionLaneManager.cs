namespace TraceFlow.LogProcessor.Services.Queue;

public interface IPartitionLaneManager
{
    ValueTask EnqueueAsync(
        PendingLogEvent pendingEvent,
        CancellationToken cancellationToken);

    Task StopAsync(
        CancellationToken cancellationToken);

    Task StopPartitionsAsync(
        IReadOnlyCollection<TopicPartition> partitions,
        CancellationToken cancellationToken);
}
