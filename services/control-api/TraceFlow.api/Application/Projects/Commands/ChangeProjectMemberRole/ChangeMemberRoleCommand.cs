using MediatR;

namespace TraceFlow.Api.Application.Projects.Commands.ChangeProjectMemberRole;

public record ChangeProjectMemberRoleCommand(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid MemberId,
    Ulid ActorUserId,
    string Role
) : IRequest<ChangeProjectMemberRoleResponse>;