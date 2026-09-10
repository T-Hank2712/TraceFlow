using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Workspaces.Commands.CancelInvitation;

public class CancelInvitationCommandHandler
    : IRequestHandler<CancelInvitationCommand, CancelInvitationResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly WorkspaceAccessService _workspaceAccess;

    public CancelInvitationCommandHandler(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccess)
    {
        _dbContext = dbContext;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<CancelInvitationResponse> Handle(
        CancelInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var actorMembership = await _workspaceAccess.GetActiveMembershipAsync(
            request.WorkspaceId,
            request.ActorUserId,
            "Workspace invitation not found.",
            cancellationToken);

        _workspaceAccess.EnsureWorkspaceIsActive(
            actorMembership.Workspace,
            "Archived workspace cannot be modified.");

        _workspaceAccess.EnsureWorkspaceManager(
            actorMembership,
            "You do not have permission to cancel workspace invitations.");

        var invitation = await _dbContext.WorkspaceInvitations
            .FirstOrDefaultAsync(
                invitation =>
                    invitation.Id == request.InvitationId &&
                    invitation.WorkspaceId == request.WorkspaceId,
                cancellationToken);

        if (invitation is null)
        {
            throw new NotFoundException("Workspace invitation not found.");
        }

        if (invitation.Status != InvitationStatuses.Pending)
        {
            throw new ConflictException("Only pending invitation can be cancelled.");
        }

        invitation.Cancel();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CancelInvitationResponse(
            invitation.Id,
            invitation.WorkspaceId,
            invitation.Status,
            invitation.UpdatedAt);
    }
}
