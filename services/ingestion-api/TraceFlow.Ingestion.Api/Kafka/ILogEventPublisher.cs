using TraceFlow.Ingestion.Api.Ingestion;

namespace TraceFlow.Ingestion.Api.Kafka;

public interface ILogEventPublisher
{
    Task PublishAsync(EnrichedLogEvent logEvent, CancellationToken cancellationToken);
}