namespace TraceFlow.Api.Application.Common.Logs;

public interface ILogSearchReader
{
    Task<SearchLogsResponse> SearchAsync(
        SearchLogsQuery query,
        CancellationToken cancellationToken);
}
