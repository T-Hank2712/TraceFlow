using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Commands.DeclineProjectInvitation;

public class DeclineProjectInvitationCommandHandler
    : IRequestHandler<DeclineProjectInvitationCommand, DeclineProjectInvitationResponse>
{
    private readonly AppDbContext _dbContext;

    public DeclineProjectInvitationCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DeclineProjectInvitationResponse> Handle(
        DeclineProjectInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var invitation = await _dbContext.ProjectInvitations
            .Include(invitation => invitation.Workspace)
            .Include(invitation => invitation.Project)
            .FirstOrDefaultAsync(
                invitation =>
                    invitation.Id == request.InvitationId &&
                    invitation.InvitedUserId == request.UserId,
                cancellationToken);

        if (invitation is null)
        {
            throw new NotFoundException("Invitation not found.");
        }

        if (invitation.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace invitation cannot be declined.");
        }

        if (invitation.Project.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived project invitation cannot be declined.");
        }

        invitation.Decline();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeclineProjectInvitationResponse(
            invitation.Id,
            invitation.WorkspaceId,
            invitation.ProjectId,
            invitation.Status);
    }
}