namespace TraceFlow.Ingestion.Api.Kafka;

public interface ILogEventPublisher
{
    Task PublishAsync(EnrichedLogEvent logEvent, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchLogItemResult>> PublishAsync(
        IReadOnlyList<(int Index, EnrichedLogEvent Event)> logEvents, 
        CancellationToken cancellationToken = default);
}
