using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Commands.CancelProjectInvitation;

public class CancelProjectInvitationCommandHandler
    : IRequestHandler<CancelProjectInvitationCommand, CancelProjectInvitationResponse>
{
    private readonly AppDbContext _dbContext;

    public CancelProjectInvitationCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CancelProjectInvitationResponse> Handle(
        CancelProjectInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var workspaceMembership = await _dbContext.WorkspaceMembers
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.ActorUserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (workspaceMembership is null)
        {
            throw new NotFoundException("Project invitation not found.");
        }

        if (workspaceMembership.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be modified.");
        }

        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(
                project =>
                    project.Id == request.ProjectId &&
                    project.WorkspaceId == request.WorkspaceId &&
                    project.Status == ResourceStatuses.Active,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException("Project invitation not found.");
        }

        var isWorkspaceManager =
            workspaceMembership.Role is WorkspaceMemberRoles.Owner or WorkspaceMemberRoles.Admin;

        var isProjectManager = await _dbContext.ProjectMembers
            .AnyAsync(
                member =>
                    member.ProjectId == request.ProjectId &&
                    member.UserId == request.ActorUserId &&
                    member.Role == ProjectMemberRoles.Manager &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (!isWorkspaceManager && !isProjectManager)
        {
            throw new ForbiddenException("You do not have permission to cancel project invitations.");
        }

        var invitation = await _dbContext.ProjectInvitations
            .FirstOrDefaultAsync(
                invitation =>
                    invitation.Id == request.InvitationId &&
                    invitation.WorkspaceId == request.WorkspaceId &&
                    invitation.ProjectId == request.ProjectId,
                cancellationToken);

        if (invitation is null)
        {
            throw new NotFoundException("Project invitation not found.");
        }

        if (invitation.Status != InvitationStatuses.Pending)
        {
            throw new ConflictException("Only pending invitation can be cancelled.");
        }

        invitation.Cancel();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CancelProjectInvitationResponse(
            invitation.Id,
            invitation.WorkspaceId,
            invitation.ProjectId,
            invitation.Status,
            invitation.UpdatedAt);
    }
}