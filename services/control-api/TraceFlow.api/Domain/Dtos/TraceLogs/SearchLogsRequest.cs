namespace TraceFlow.Api.Domain.Dtos.Logs;

public sealed record SearchLogsRequest(
    Ulid? ApplicationId,
    string? Environment,
    string? Level,
    string? Service,
    string? TraceId,
    string? CorrelationId,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 20);