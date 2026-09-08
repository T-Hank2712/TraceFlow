namespace TraceFlow.Api.Application.Workspaces.Commands.InviteWorkspaceMember;

public record InviteWorkspaceMemberResponse(
    Ulid InvitationId,
    Ulid WorkspaceId,
    Ulid InvitedUserId,
    string InvitedUserName,
    string InvitedUserEmail,
    string Role,
    string Status,
    DateTime ExpiresAt);