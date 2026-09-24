using OpenSearch.Client;
using TraceFlow.LogProcessor.Contracts;

namespace TraceFlow.LogProcessor.Services.OpenSearch;

public interface ILogIndexer
{
    Task<BulkIndexResult> IndexAsync(IReadOnlyList<LogEvent> logEvents, CancellationToken cancellationToken);

    Task<BulkResponse> ExecuteBulkAsync(IReadOnlyList<LogEvent> logEvents, CancellationToken cancellationToken);
}