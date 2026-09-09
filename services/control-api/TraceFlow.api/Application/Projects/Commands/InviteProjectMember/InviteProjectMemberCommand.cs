using MediatR;

namespace TraceFlow.Api.Application.Projects.Commands.InviteProjectMember;

public record InviteProjectMemberCommand(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid InvitedByUserId,
    string Identifier,
    string Role
) : IRequest<InviteProjectMemberResponse>;