using OpenSearch.Client;
using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.Services.OpenSearch;

namespace TraceFlow.LogProcessor.IntegrationTests.Infrastructure;

public sealed class PartialFailingLogIndexer : ILogIndexer
{
    public IReadOnlyList<LogEvent> ReceivedEvents { get; private set; } =
        Array.Empty<LogEvent>();

    public Task<BulkIndexResult> IndexAsync(
        IReadOnlyList<LogEvent> logEvents,
        CancellationToken cancellationToken)
    {
        ReceivedEvents = logEvents;

        var succeeded = logEvents.Take(1).ToArray();
        var failed = logEvents.Skip(1).ToArray();

        return Task.FromResult(
            new BulkIndexResult(
                succeeded,
                failed));
    }

    public Task<BulkResponse> ExecuteBulkAsync(
        IReadOnlyList<LogEvent> logEvents,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException(
            "PartialFailingLogIndexer only supports IndexAsync in integration tests.");
    }
}