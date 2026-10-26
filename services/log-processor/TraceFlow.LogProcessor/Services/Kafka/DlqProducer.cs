namespace TraceFlow.LogProcessor.Services.Kafka;

public sealed class DlqProducer : IDlqProducer, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly KafkaOptions _kafkaOptions;
    private readonly ILogger<DlqProducer> _logger;
    public DlqProducer(IOptions<KafkaOptions> kafkaOptions, ILogger<DlqProducer> logger)
    {
        _kafkaOptions = kafkaOptions.Value;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            Acks = Acks.All
        };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }
    public async Task PublishAsync(IReadOnlyCollection<DlqLogEvent> events, CancellationToken cancellationToken)
    {
        if (events.Count == 0)
        {
            return;
        }

        using var concurrency = new SemaphoreSlim(
            _kafkaOptions.DlqPublishConcurrency);

        var publishTasks = new List<Task>(
            events.Count);

        try
        {
            foreach (var dlqEvent in events)
            {
                await concurrency.WaitAsync(
                    cancellationToken);

                publishTasks.Add(
                    PublishOneAsync(
                        dlqEvent,
                        concurrency,
                        cancellationToken));
            }
            await Task.WhenAll(publishTasks);
        }
        catch
        {
            try
            {
                await Task.WhenAll(publishTasks);
            }
            catch (Exception)
            {

            }

            throw;
        }

        _logger.LogInformation("Published failed log events to DLQ. Count: {Count}, Topic: {Topic}",
        events.Count, _kafkaOptions.DlqTopic);
    }
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(_kafkaOptions.ProducerFlushTimeoutSeconds));
        _producer.Dispose();
    }
    private async Task PublishOneAsync(
        DlqLogEvent dlqEvent,
        SemaphoreSlim concurrency,
        CancellationToken cancellationToken)
    {
        try
        {
            var value = JsonSerializer.Serialize(
                dlqEvent);

            await _producer.ProduceAsync(
                _kafkaOptions.DlqTopic,
                new Message<Null, string>
                {
                    Value = value
                },
                cancellationToken);
        }
        finally
        {
            concurrency.Release();
        }
    }
}
