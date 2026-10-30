using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.Services.Kafka;

namespace TraceFlow.LogProcessor.IntegrationTests.Infrastructure;

public sealed class RecordingDlqProducer : IDlqProducer
{
    private readonly List<DlqLogEvent> _events = [];

    public IReadOnlyList<DlqLogEvent> PublishedEvents => _events;

    public Task PublishAsync(
        IReadOnlyCollection<DlqLogEvent> events,
        CancellationToken cancellationToken)
    {
        _events.AddRange(events);

        return Task.CompletedTask;
    }
}