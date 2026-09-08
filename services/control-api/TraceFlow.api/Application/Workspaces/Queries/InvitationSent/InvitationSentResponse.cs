namespace TraceFlow.Api.Application.Workspaces.Queries.InvitationSent;

public record InvitationSentResponse(
    Ulid Id,
    Ulid WorkspaceId,
    Ulid InvitedUserId,
    string InvitedUserName,
    string InvitedUserEmail,
    Ulid InvitedByUserId,
    string InvitedByUserName,
    string Role,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt);