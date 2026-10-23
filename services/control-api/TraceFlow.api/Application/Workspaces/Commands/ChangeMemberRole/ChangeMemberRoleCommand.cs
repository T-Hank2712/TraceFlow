namespace TraceFlow.Api.Application.Workspaces.Commands.ChangeMemberRole;

public record ChangeMemberRoleCommand(
    Ulid MemberId,
    Ulid ActorUserId,
    string Role
) : IRequest<ChangeMemberRoleResponse>;
