using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Commands.CancelInvitation;

public record CancelInvitationCommand(
    Ulid WorkspaceId,
    Ulid InvitationId,
    Ulid ActorUserId
) : IRequest<CancelInvitationResponse>;