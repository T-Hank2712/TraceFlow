namespace TraceFlow.Api.Application.Workspaces.Commands.CancelInvitation;

public record CancelInvitationResponse(
    Ulid InvitationId,
    Ulid WorkspaceId,
    string Status,
    DateTimeOffset UpdatedAt);
