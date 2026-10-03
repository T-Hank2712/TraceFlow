using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.Services.Kafka;
using TraceFlow.LogProcessor.Services.Queue;

namespace TraceFlow.LogProcessor.Workers;

public sealed class LogConsumerWorker : BackgroundService
{
    private readonly IKafkaConsumer _kafkaConsumer;
    private readonly ILogger<LogConsumerWorker> _logger;
    private readonly IPendingLogEventQueue _queue;

    public LogConsumerWorker(
        IKafkaConsumer kafkaConsumer,
        ILogger<LogConsumerWorker> logger,
        IPendingLogEventQueue queue)
    {
        _kafkaConsumer = kafkaConsumer;
        _logger = logger;
        _queue = queue;
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
        await _queue.WriteAsync(
            pendingEvent,
            cancellationToken);

        return new BatchProcessResult([]);
    }
}