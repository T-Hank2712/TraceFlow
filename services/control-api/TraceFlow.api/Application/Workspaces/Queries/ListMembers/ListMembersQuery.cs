using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Queries.ListMembers;

public record ListMembersQuery(
    Ulid WorkspaceId,
    Ulid UserId
) : IRequest<IReadOnlyList<WorkspaceMemberResponse>>;