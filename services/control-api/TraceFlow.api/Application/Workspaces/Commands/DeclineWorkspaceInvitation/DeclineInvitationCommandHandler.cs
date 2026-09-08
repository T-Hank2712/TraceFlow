using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Application.Workspaces.Commands.DeclineWorkspaceInvitation;

public class DeclineInvitationCommandHandler
    : IRequestHandler<DeclineInvitationCommand, DeclineInvitationResponse>
{
    private readonly AppDbContext _dbContext;

    public DeclineInvitationCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DeclineInvitationResponse> Handle(
        DeclineInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var invitation = await _dbContext.WorkspaceInvitations
            .Include(invitation => invitation.Workspace)
            .FirstOrDefaultAsync(
                invitation =>
                    invitation.Id == request.InvitationId &&
                    invitation.InvitedUserId == request.UserId,
                cancellationToken);

        if (invitation is null)
        {
            throw new NotFoundException("Invitation not found.");
        }

        if (invitation.Workspace.Status == WorkspaceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace invitation cannot be declined.");
        }

        invitation.Decline();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeclineInvitationResponse(
            invitation.Id,
            invitation.WorkspaceId,
            invitation.Status);
    }
}