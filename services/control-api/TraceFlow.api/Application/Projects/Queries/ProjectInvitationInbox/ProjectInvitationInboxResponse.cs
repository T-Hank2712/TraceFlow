namespace TraceFlow.Api.Application.Projects.Queries.ProjectInvitationInbox;

public record ProjectInvitationInboxResponse(
    Ulid InvitationId,
    Ulid WorkspaceId,
    string WorkspaceName,
    Ulid ProjectId,
    string ProjectName,
    string ProjectSlug,
    Ulid InvitedByUserId,
    string InvitedByUserName,
    string Role,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt);