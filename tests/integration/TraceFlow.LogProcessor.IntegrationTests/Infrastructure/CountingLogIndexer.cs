using OpenSearch.Client;
using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.Services.OpenSearch;

namespace TraceFlow.LogProcessor.IntegrationTests.Infrastructure;

public sealed class CountingLogIndexer : ILogIndexer
{
    private readonly List<LogEvent> _indexedEvents = [];

    public IReadOnlyList<LogEvent> IndexedEvents => _indexedEvents;

    public Task<BulkIndexResult> IndexAsync(
        IReadOnlyList<LogEvent> logEvents,
        CancellationToken cancellationToken)
    {
        _indexedEvents.AddRange(logEvents);

        return Task.FromResult(
            new BulkIndexResult(
                logEvents,
                Array.Empty<LogEvent>()));
    }

    public Task<BulkResponse> ExecuteBulkAsync(
        IReadOnlyList<LogEvent> logEvents,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException(
            "CountingLogIndexer only supports IndexAsync in integration tests.");
    }
}