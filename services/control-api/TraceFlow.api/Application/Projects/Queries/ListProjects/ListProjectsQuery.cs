using MediatR;

namespace TraceFlow.Api.Application.Projects.Queries.ListProjects;

public record ListProjectsQuery(
    Ulid WorkspaceId,
    Ulid UserId
) : IRequest<IReadOnlyList<ProjectSummaryResponse>>;