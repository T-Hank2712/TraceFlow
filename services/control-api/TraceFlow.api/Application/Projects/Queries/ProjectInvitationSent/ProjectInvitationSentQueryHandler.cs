using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Queries.ProjectInvitationSent;

public class ProjectInvitationSentQueryHandler
    : IRequestHandler<ProjectInvitationSentQuery, IReadOnlyList<ProjectInvitationSentResponse>>
{
    private readonly AppDbContext _dbContext;

    public ProjectInvitationSentQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProjectInvitationSentResponse>> Handle(
        ProjectInvitationSentQuery request,
        CancellationToken cancellationToken)
    {
        var workspaceMembership = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Include(member => member.Workspace)
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == request.UserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (workspaceMembership is null)
        {
            throw new NotFoundException("Project not found.");
        }

        if (workspaceMembership.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be accessed.");
        }

        var projectExists = await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(
                project =>
                    project.Id == request.ProjectId &&
                    project.WorkspaceId == request.WorkspaceId &&
                    project.Status == ResourceStatuses.Active,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException("Project not found.");
        }

        var isWorkspaceManager =
            workspaceMembership.Role is WorkspaceMemberRoles.Owner or WorkspaceMemberRoles.Admin;

        var isProjectManager = await _dbContext.ProjectMembers
            .AsNoTracking()
            .AnyAsync(
                member =>
                    member.ProjectId == request.ProjectId &&
                    member.UserId == request.UserId &&
                    member.Role == ProjectMemberRoles.Manager &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (!isWorkspaceManager && !isProjectManager)
        {
            throw new ForbiddenException("You do not have permission to view project invitations.");
        }

        return await _dbContext.ProjectInvitations
            .AsNoTracking()
            .Where(invitation =>
                invitation.WorkspaceId == request.WorkspaceId &&
                invitation.ProjectId == request.ProjectId)
            .OrderByDescending(invitation => invitation.CreatedAt)
            .Select(invitation => new ProjectInvitationSentResponse(
                invitation.Id,
                invitation.WorkspaceId,
                invitation.ProjectId,
                invitation.InvitedUserId,
                invitation.InvitedUser.UserName,
                invitation.InvitedUser.Email,
                invitation.InvitedByUserId,
                invitation.InvitedByUser.UserName,
                invitation.Role,
                invitation.Status,
                invitation.ExpiresAt,
                invitation.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}