namespace TraceFlow.Api.Application.Projects.Commands.ChangeProjectMemberRole;

public record ChangeProjectMemberRoleCommand(
    Ulid MemberId,
    Ulid ActorUserId,
    string Role
) : IRequest<ChangeProjectMemberRoleResponse>;
