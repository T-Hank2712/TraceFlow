namespace TraceFlow.LogProcessor.Workers;

public sealed class LogConsumerWorker : BackgroundService
{
    private readonly IKafkaConsumer _kafkaConsumer;
    private readonly ILogger<LogConsumerWorker> _logger;
    private readonly IPartitionLaneManager _laneManager;
    public LogConsumerWorker(
        IKafkaConsumer kafkaConsumer,
        ILogger<LogConsumerWorker> logger,
        IPartitionLaneManager laneManager)
    {
        _kafkaConsumer = kafkaConsumer;
        _logger = logger;
        _laneManager = laneManager;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("TraceFlow Kafka consumer started.");

        await _kafkaConsumer.ConsumeAsync(
            EnqueueAsync,
            stoppingToken);

        _logger.LogInformation("TraceFlow Kafka consumer stopped.");
    }

    private async Task<BatchProcessResult> EnqueueAsync(
        PendingLogEvent pendingEvent,
        CancellationToken cancellationToken)
    {
        await _laneManager.EnqueueAsync(
            pendingEvent,
            cancellationToken);

        return new BatchProcessResult([]);
    }
}
