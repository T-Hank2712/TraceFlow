namespace TraceFlow.Api.Application.Workspaces.Commands.AcceptWorkspaceInvitation;

public record AcceptInvitationResponse(
    Ulid InvitationId,
    Ulid WorkspaceId,
    Ulid MemberId,
    string Role,
    string Status);