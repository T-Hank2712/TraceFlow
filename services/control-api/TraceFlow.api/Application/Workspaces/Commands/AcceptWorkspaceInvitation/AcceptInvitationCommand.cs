using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Commands.AcceptWorkspaceInvitation;

public record AcceptInvitationCommand(
    Ulid InvitationId,
    Ulid UserId
) : IRequest<AcceptInvitationResponse>;