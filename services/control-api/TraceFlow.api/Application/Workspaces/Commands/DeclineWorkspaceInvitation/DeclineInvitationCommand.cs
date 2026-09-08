using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Commands.DeclineWorkspaceInvitation;

public record DeclineInvitationCommand(
    Ulid InvitationId,
    Ulid UserId
) : IRequest<DeclineInvitationResponse>;