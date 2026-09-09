using MediatR;

namespace TraceFlow.Api.Application.Projects.Commands.DeclineProjectInvitation;

public record DeclineProjectInvitationCommand(
    Ulid InvitationId,
    Ulid UserId
) : IRequest<DeclineProjectInvitationResponse>;