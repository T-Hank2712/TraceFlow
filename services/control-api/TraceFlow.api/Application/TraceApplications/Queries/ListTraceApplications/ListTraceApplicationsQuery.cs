using MediatR;

namespace TraceFlow.Api.Application.TraceApplications.Queries.ListTraceApplications;

public record ListTraceApplicationsQuery(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid UserId
) : IRequest<IReadOnlyList<TraceApplicationSummaryResponse>>;