using TraceFlow.Ingestion.Api.Contracts.BatchLog;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.Kafka;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

public sealed class FailingLogEventPublisher : ILogEventPublisher
{
    public Task PublishAsync(
        EnrichedLogEvent logEvent,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Kafka publish failed.");
    }

    public Task<IReadOnlyList<BatchLogItemResult>> PublishAsync(
        IReadOnlyList<(int Index, EnrichedLogEvent Event)> logEvents,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<BatchLogItemResult> results = logEvents
            .Select(item => new BatchLogItemResult(
                item.Index,
                false,
                item.Event.EventId,
                "Kafka publish failed."))
            .ToList();

        return Task.FromResult(results);
    }
}