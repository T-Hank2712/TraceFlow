using MediatR;

namespace TraceFlow.Api.Application.Projects.Commands.AcceptProjectInvitation;

public record AcceptProjectInvitationCommand(
    Ulid InvitationId,
    Ulid UserId
) : IRequest<AcceptProjectInvitationResponse>;