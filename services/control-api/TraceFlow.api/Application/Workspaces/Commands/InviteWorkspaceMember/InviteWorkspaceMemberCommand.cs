using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Commands.InviteWorkspaceMember;

public record InviteWorkspaceMemberCommand(
    Ulid WorkspaceId,
    Ulid InvitedByUserId,
    string Identifier,
    string Role
) : IRequest<InviteWorkspaceMemberResponse>;