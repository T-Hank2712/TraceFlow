using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Queries.ProjectInvitationSent;

public class ProjectInvitationSentQueryHandler
    : IRequestHandler<ProjectInvitationSentQuery, IReadOnlyList<ProjectInvitationSentResponse>>
{
    private readonly AppDbContext _dbContext;
    private readonly ProjectAccessService _projectAccess;

    public ProjectInvitationSentQueryHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess)
    {
        _dbContext = dbContext;
        _projectAccess = projectAccess;
    }

    public async Task<IReadOnlyList<ProjectInvitationSentResponse>> Handle(
        ProjectInvitationSentQuery request,
        CancellationToken cancellationToken)
    {
        var access = await _projectAccess.GetProjectAccessAsync(
            request.WorkspaceId,
            request.ProjectId,
            request.UserId,
            "Project not found.",
            cancellationToken,
            "Archived workspace cannot be accessed.");

        _projectAccess.EnsureProjectManager(
            access,
            "You do not have permission to view project invitations.");

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
