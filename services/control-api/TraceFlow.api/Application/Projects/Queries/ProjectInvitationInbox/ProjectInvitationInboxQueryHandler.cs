namespace TraceFlow.Api.Application.Projects.Queries.ProjectInvitationInbox;

public class ProjectInvitationInboxQueryHandler(
    AppDbContext dbContext,
    TimeProvider timeProvider)
    : IRequestHandler<ProjectInvitationInboxQuery, IReadOnlyList<ProjectInvitationInboxResponse>>
{

    private readonly AppDbContext _dbContext = dbContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<IReadOnlyList<ProjectInvitationInboxResponse>> Handle(
        ProjectInvitationInboxQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ProjectInvitations
            .AsNoTracking()
            .Where(invitation =>
                invitation.InvitedUserId == request.UserId &&
                invitation.Status == InvitationStatuses.Pending &&
                invitation.ExpiresAt > _timeProvider.GetUtcNow() &&
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
