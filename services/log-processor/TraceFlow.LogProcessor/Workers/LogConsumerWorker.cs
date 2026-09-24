using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.Services.Batching;
using TraceFlow.LogProcessor.Services.Kafka;

namespace TraceFlow.LogProcessor.Workers;

public sealed class LogConsumerWorker : BackgroundService
{
    private readonly IKafkaConsumer _kafkaConsumer;
    private readonly ILogger<LogConsumerWorker> _logger;
    private readonly IBatchProcessor _batchProcessor;

    public LogConsumerWorker(
        IKafkaConsumer kafkaConsumer,
        ILogger<LogConsumerWorker> logger,
        IBatchProcessor batchProcessor)
    {
        _kafkaConsumer = kafkaConsumer;
        _logger = logger;
        _batchProcessor = batchProcessor;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("TraceFlow Log Processor started.");

        await _kafkaConsumer.ConsumeAsync(
            ProcessAsync,
            stoppingToken);

        _logger.LogInformation("TraceFlow Log Processor stopped.");
    }

    private async Task<BatchProcessResult> ProcessAsync(
        PendingLogEvent pendingEvent,
        CancellationToken cancellationToken)
    {
        return await _batchProcessor.AddAsync(
            pendingEvent,
            cancellationToken);
    }
}
