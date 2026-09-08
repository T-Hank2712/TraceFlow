using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Commands.ChangeMemberRole;

public record ChangeMemberRoleCommand(
    Ulid WorkspaceId,
    Ulid MemberId,
    Ulid ActorUserId,
    string Role
) : IRequest<ChangeMemberRoleResponse>;