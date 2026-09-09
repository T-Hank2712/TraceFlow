namespace TraceFlow.Api.Application.Workspaces.Commands.CancelInvitation;

public record CancelInvitationResponse(
    Ulid InvitationId,
    Ulid WorkspaceId,
    string Status,
    DateTime UpdatedAt);