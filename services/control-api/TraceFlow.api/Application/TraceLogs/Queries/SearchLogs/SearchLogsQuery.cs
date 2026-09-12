using MediatR;

namespace TraceFlow.Api.Application.Logs.Queries.SearchLogs;

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
    DateTime? From,
    DateTime? To,
    int Page,
    int PageSize
) : IRequest<SearchLogsResponse>;