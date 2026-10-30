namespace TraceFlow.Api.Application.Projects.Queries.ListProjectMembers;

public record ListProjectMembersQuery(
    Ulid ProjectId,
    Ulid UserId
) : IRequest<IReadOnlyList<ProjectMemberResponse>>;
