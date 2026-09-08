namespace TraceFlow.Api.Application.Workspaces.Commands.DeclineWorkspaceInvitation;

public record DeclineInvitationResponse(
    Ulid InvitationId,
    Ulid WorkspaceId,
    string Status);