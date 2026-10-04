namespace TraceFlow.Api.Application.TraceLogs.Queries.SearchLogs;

public sealed record SearchLogsQuery(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid UserId,
    Ulid? ApplicationId,
    string? Environment,
    string? Level,
    string? Service,
    string? TraceId,
    string? CorrelationId,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int PageSize
) : IRequest<SearchLogsResponse>;
