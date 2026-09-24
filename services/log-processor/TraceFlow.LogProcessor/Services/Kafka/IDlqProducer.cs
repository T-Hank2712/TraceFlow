using TraceFlow.LogProcessor.Contracts;

namespace TraceFlow.LogProcessor.Services.Kafka;

public interface IDlqProducer
{
    Task PublishAsync(IReadOnlyCollection<DlqLogEvent> events, CancellationToken cancellationToken);
}
