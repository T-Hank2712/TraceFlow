namespace TraceFlow.Api.Application.Projects.Commands.AcceptProjectInvitation;

public record AcceptProjectInvitationResponse(
    Ulid InvitationId,
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid WorkspaceMemberId,
    Ulid ProjectMemberId,
    string ProjectRole,
    string InvitationStatus);