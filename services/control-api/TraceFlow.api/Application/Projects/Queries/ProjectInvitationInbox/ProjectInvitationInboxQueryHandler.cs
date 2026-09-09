using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Queries.ProjectInvitationInbox;

public class ProjectInvitationInboxQueryHandler
    : IRequestHandler<ProjectInvitationInboxQuery, IReadOnlyList<ProjectInvitationInboxResponse>>
{
    private readonly AppDbContext _dbContext;

    public ProjectInvitationInboxQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProjectInvitationInboxResponse>> Handle(
        ProjectInvitationInboxQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ProjectInvitations
            .AsNoTracking()
            .Where(invitation =>
                invitation.InvitedUserId == request.UserId &&
                invitation.Status == InvitationStatuses.Pending &&
                invitation.ExpiresAt > DateTime.UtcNow &&
                invitation.Workspace.Status == ResourceStatuses.Active &&
                invitation.Project.Status == ResourceStatuses.Active)
            .OrderByDescending(invitation => invitation.CreatedAt)
            .Select(invitation => new ProjectInvitationInboxResponse(
                invitation.Id,
                invitation.WorkspaceId,
                invitation.Workspace.Name,
                invitation.ProjectId,
                invitation.Project.Name,
                invitation.Project.Slug,
                invitation.InvitedByUserId,
                invitation.InvitedByUser.UserName,
                invitation.Role,
                invitation.Status,
                invitation.ExpiresAt,
                invitation.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}