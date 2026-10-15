namespace TraceFlow.LogProcessor.Services.Queue;

public sealed class PartitionLaneManager
    : IPartitionLaneManager
{
    private readonly ConcurrentDictionary<TopicPartition, PartitionLane> _lanes = [];

    private readonly IBatchProcessorFactory _batchProcessorFactory;
    private readonly IOffsetCoordinator _offsetCoordinator;
    private readonly IOffsetCommitSignal _offsetCommitSignal;
    private readonly IOptions<ProcessorOptions> _processorOptions;
    private readonly ILoggerFactory _loggerFactory;

    public PartitionLaneManager(
        IBatchProcessorFactory batchProcessorFactory,
        IOffsetCoordinator offsetCoordinator,
        IOffsetCommitSignal offsetCommitSignal,
        IOptions<ProcessorOptions> processorOptions,
        ILoggerFactory loggerFactory)
    {
        _batchProcessorFactory = batchProcessorFactory;
        _offsetCoordinator = offsetCoordinator;
        _offsetCommitSignal = offsetCommitSignal;
        _processorOptions = processorOptions;
        _loggerFactory = loggerFactory;
    }

    public ValueTask EnqueueAsync(
        PendingLogEvent pendingEvent,
        CancellationToken cancellationToken)
    {
        var topicPartition =
            pendingEvent.Offset.TopicPartition;

        var lane = _lanes.GetOrAdd(
            topicPartition,
            CreateLane);

        return lane.EnqueueAsync(
            pendingEvent,
            cancellationToken);
    }

    private PartitionLane CreateLane(
        TopicPartition topicPartition)
    {
        var lane = new PartitionLane(
            topicPartition,
            _processorOptions.Value.QueueCapacity,
            TimeSpan.FromMilliseconds(
                _processorOptions.Value.FlushIntervalMs),
            _batchProcessorFactory.Create(),
            _offsetCoordinator,
            _offsetCommitSignal,
            _loggerFactory.CreateLogger<
                PartitionLane>());

        lane.Start();

        return lane;
    }

    public async Task StopAsync(
        CancellationToken cancellationToken)
    {
        var lanes = _lanes.Values.ToArray();

        await Task.WhenAll(
            lanes.Select(lane =>
                lane.StopAsync(cancellationToken)));
    }
}