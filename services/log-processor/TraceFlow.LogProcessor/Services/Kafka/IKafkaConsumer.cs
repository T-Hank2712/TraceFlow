using TraceFlow.LogProcessor.Contracts;

namespace TraceFlow.LogProcessor.Services.Kafka;

public interface IKafkaConsumer
{
    Task ConsumeAsync(
        Func<LogEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken);
}