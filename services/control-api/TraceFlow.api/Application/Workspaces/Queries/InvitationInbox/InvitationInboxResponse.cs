namespace TraceFlow.Api.Application.Workspaces.Queries.InvitationInbox;

public record InvitationInboxResponse(
    Ulid Id,
    Ulid WorkspaceId,
    string WorkspaceName,
    string WorkspaceSlug,
    Ulid InvitedByUserId,
    string InvitedByUserName,
    string Role,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt
    );