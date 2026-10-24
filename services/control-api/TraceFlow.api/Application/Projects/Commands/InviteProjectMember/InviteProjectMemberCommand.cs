namespace TraceFlow.Api.Application.Projects.Commands.InviteProjectMember;

public record InviteProjectMemberCommand(
    Ulid ProjectId,
    Ulid InvitedByUserId,
    string Identifier,
    string Role
) : IRequest<InviteProjectMemberResponse>;
