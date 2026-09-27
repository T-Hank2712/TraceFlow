using TraceFlow.Ingestion.Api.Contracts.BatchLog;
using TraceFlow.Ingestion.Api.Contracts.Log;

namespace TraceFlow.Ingestion.Api.Kafka;

public interface ILogEventPublisher
{
    Task PublishAsync(EnrichedLogEvent logEvent, CancellationToken cancellationToken = default);
    IReadOnlyList<BatchLogItemResult> Publish(
    IReadOnlyList<(int Index, EnrichedLogEvent Event)> logEvents, CancellationToken cancellationToken = default);
}
