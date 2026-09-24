using TraceFlow.LogProcessor.Contracts;

namespace TraceFlow.LogProcessor.Services.Kafka;

public interface IKafkaConsumer
{
    Task ConsumeAsync(
        Func<PendingLogEvent, CancellationToken, Task<BatchProcessResult>> handler,
        CancellationToken cancellationToken);
}
