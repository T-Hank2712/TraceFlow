using MediatR;

namespace TraceFlow.Api.Application.Projects.Queries.ListProjectMembers;

public record ListProjectMembersQuery(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid UserId
) : IRequest<IReadOnlyList<ProjectMemberResponse>>;