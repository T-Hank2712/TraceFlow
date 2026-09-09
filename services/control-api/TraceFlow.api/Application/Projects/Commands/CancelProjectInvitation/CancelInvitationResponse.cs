namespace TraceFlow.Api.Application.Projects.Commands.CancelProjectInvitation;

public record CancelProjectInvitationResponse(
    Ulid InvitationId,
    Ulid WorkspaceId,
    Ulid ProjectId,
    string Status,
    DateTime UpdatedAt);