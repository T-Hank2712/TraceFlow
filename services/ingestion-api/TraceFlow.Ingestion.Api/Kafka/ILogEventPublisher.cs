using TraceFlow.Ingestion.Api.Contracts.Log;

namespace TraceFlow.Ingestion.Api.Kafka;

public interface ILogEventPublisher
{
    Task PublishAsync(EnrichedLogEvent logEvent, CancellationToken cancellationToken);
    Task PublishAsync(IReadOnlyList<EnrichedLogEvent> logEvents, CancellationToken cancellationToken);
}
