namespace TraceFlow.Api.Application.Common.Logs;

public interface ILogSearchReader
{
    Task<SearchLogsResponse> SearchAsync(
        SearchLogsQuery query,
        CancellationToken cancellationToken);

    Task<bool> HasLogsAsync(
        Ulid workspaceId,
        Ulid projectId,
        Ulid applicationId,
        CancellationToken cancellationToken);
}
