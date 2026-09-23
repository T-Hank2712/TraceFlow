using OpenSearch.Client;
using TraceFlow.LogProcessor.Contracts;

namespace TraceFlow.LogProcessor.Services.OpenSearch;

public interface ILogIndexer
{
    Task IndexAsync(IReadOnlyCollection<LogEvent> logEvents, CancellationToken cancellationToken);

    Task<BulkResponse> ExecuteBulkAsync(IReadOnlyCollection<LogEvent> logEvents, CancellationToken cancellationToken);
}