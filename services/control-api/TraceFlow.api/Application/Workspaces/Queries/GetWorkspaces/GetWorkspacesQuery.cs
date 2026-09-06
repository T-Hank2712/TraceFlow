using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Queries.GetWorkspaces;

public record GetWorkspacesQuery(Ulid UserId)
    : IRequest<IReadOnlyList<WorkspaceSummaryResponse>>;