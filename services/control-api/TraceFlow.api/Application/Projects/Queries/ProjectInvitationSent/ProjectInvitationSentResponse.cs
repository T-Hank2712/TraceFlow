namespace TraceFlow.Api.Application.Projects.Queries.ProjectInvitationSent;

public record ProjectInvitationSentResponse(
    Ulid InvitationId,
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid InvitedUserId,
    string InvitedUserName,
    string InvitedUserEmail,
    Ulid InvitedByUserId,
    string InvitedByUserName,
    string Role,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt);