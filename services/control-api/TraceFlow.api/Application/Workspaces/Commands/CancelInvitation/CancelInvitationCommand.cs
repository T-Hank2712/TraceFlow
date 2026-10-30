namespace TraceFlow.Api.Application.Workspaces.Commands.CancelInvitation;

public record CancelInvitationCommand(
    Ulid InvitationId,
    Ulid ActorUserId
) : IRequest<CancelInvitationResponse>;
