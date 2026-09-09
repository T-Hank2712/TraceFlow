using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Workspaces.Commands.CancelInvitation;

public class CancelInvitationCommandHandler
    : IRequestHandler<CancelInvitationCommand, CancelInvitationResponse>
{
    private readonly AppDbContext _dbContext;

    public CancelInvitationCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CancelInvitationResponse> Handle(
        CancelInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var actorMembership = await _dbContext.WorkspaceMembers
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.ActorUserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (actorMembership is null)
        {
            throw new NotFoundException("Workspace invitation not found.");
        }

        if (actorMembership.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be modified.");
        }

        if (actorMembership.Role is not WorkspaceMemberRoles.Owner and not WorkspaceMemberRoles.Admin)
        {
            throw new ForbiddenException("You do not have permission to cancel workspace invitations.");
        }

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