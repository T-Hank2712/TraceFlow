namespace TraceFlow.Api.Application.Projects.Commands.CancelProjectInvitation;

public record CancelProjectInvitationCommand(
    Ulid ProjectId,
    Ulid InvitationId,
    Ulid ActorUserId
) : IRequest<CancelProjectInvitationResponse>;
