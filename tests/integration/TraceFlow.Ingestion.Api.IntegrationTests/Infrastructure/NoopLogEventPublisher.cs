using TraceFlow.Ingestion.Api.Contracts.BatchLog;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.Kafka;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

public sealed class NoopLogEventPublisher : ILogEventPublisher
{
    private readonly List<EnrichedLogEvent> _publishedEvents = [];

    public IReadOnlyList<EnrichedLogEvent> PublishedEvents => _publishedEvents;

    public Task PublishAsync(
        EnrichedLogEvent logEvent,
        CancellationToken cancellationToken = default)
    {
        _publishedEvents.Add(logEvent);

        return Task.CompletedTask;
    }

    public IReadOnlyList<BatchLogItemResult> Publish(
        IReadOnlyList<(int Index, EnrichedLogEvent Event)> logEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var item in logEvents)
        {
            _publishedEvents.Add(item.Event);
        }

        return logEvents
            .Select(item => new BatchLogItemResult(
                item.Index,
                true,
                item.Event.EventId,
                null))
            .ToList();
    }
}