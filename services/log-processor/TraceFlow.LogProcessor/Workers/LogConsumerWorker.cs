using TraceFlow.LogProcessor.Services.Kafka;

namespace TraceFlow.LogProcessor.Workers;

public sealed class LogConsumerWorker : BackgroundService
{
    private readonly IKafkaConsumer _kafkaConsumer;
    private readonly ILogger<LogConsumerWorker> _logger;

    public LogConsumerWorker(
        IKafkaConsumer kafkaConsumer,
        ILogger<LogConsumerWorker> logger)
    {
        _kafkaConsumer = kafkaConsumer;
        _logger = logger;
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

    private Task ProcessAsync(
        Contracts.LogEvent logEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Consumed log event {EventId} from service {Service}: {Message}",
            logEvent.EventId,
            logEvent.Service,
            logEvent.Message);

        return Task.CompletedTask;
    }
}
