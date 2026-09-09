namespace TraceFlow.Api.Application.Projects.Commands.InviteProjectMember;

public record InviteProjectMemberResponse(
    Ulid InvitationId,
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid InvitedUserId,
    string InvitedUserName,
    string InvitedUserEmail,
    string Role,
    string Status,
    DateTime ExpiresAt);