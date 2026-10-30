namespace TraceFlow.Api.Application.TraceLogs.Queries.SearchLogs;

public sealed record SearchLogsResponse(
    IReadOnlyList<LogSearchItemResponse> Items,
    long Total,
    int Page,
    int PageSize);
