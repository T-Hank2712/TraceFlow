namespace TraceFlow.Api.Application.Projects.Commands.DeclineProjectInvitation;

public record DeclineProjectInvitationResponse(
    Ulid InvitationId,
    Ulid WorkspaceId,
    Ulid ProjectId,
    string Status);