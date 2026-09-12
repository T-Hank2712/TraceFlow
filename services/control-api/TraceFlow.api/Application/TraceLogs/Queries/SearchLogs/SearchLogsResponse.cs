using TraceFlow.Api.Domain.Dtos.Logs;

namespace TraceFlow.Api.Application.Logs.Queries.SearchLogs;

public sealed record SearchLogsResponse(
    IReadOnlyList<LogSearchItemResponse> Items,
    long Total,
    int Page,
    int PageSize);