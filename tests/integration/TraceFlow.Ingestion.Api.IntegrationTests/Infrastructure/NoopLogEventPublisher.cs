using TraceFlow.Ingestion.Api.Contracts.BatchLog;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.Kafka;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

public sealed class NoopLogEventPublisher : ILogEventPublisher
{
    public Task PublishAsync(
        EnrichedLogEvent logEvent,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public IReadOnlyList<BatchLogItemResult> Publish(
        IReadOnlyList<(int Index, EnrichedLogEvent Event)> logEvents,
        CancellationToken cancellationToken = default)
    {
        return logEvents
            .Select(item => new BatchLogItemResult(
                item.Index,
                true,
                item.Event.EventId,
                null))
            .ToList();
    }
}