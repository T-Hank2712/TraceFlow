namespace TraceFlow.Api.Application.TraceApplications.Queries.ListTraceApplications;

public record ListTraceApplicationsQuery(
    Ulid ProjectId,
    Ulid UserId
) : IRequest<IReadOnlyList<TraceApplicationSummaryResponse>>;
