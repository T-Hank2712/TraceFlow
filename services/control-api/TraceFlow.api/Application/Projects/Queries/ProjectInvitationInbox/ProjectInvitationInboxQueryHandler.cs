namespace TraceFlow.Api.Application.Projects.Queries.ProjectInvitationInbox;

public class ProjectInvitationInboxQueryHandler(
    AppDbContext dbContext,
    TimeProvider timeProvider,
    InvitationExpirationService invitationExpiration)
    : IRequestHandler<ProjectInvitationInboxQuery, IReadOnlyList<ProjectInvitationInboxResponse>>
{

    private readonly AppDbContext _dbContext = dbContext;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly InvitationExpirationService _invitationExpiration = invitationExpiration;

    public async Task<IReadOnlyList<ProjectInvitationInboxResponse>> Handle(
        ProjectInvitationInboxQuery request,
        CancellationToken cancellationToken)
    {
        await _invitationExpiration.ExpireProjectInvitationsForUserAsync(
            request.UserId,
            cancellationToken);

        var utcNow = _timeProvider.GetUtcNow();

        return await _dbContext.ProjectInvitations
            .AsNoTracking()
            .Where(invitation =>
                invitation.InvitedUserId == request.UserId &&
                invitation.Status == InvitationStatuses.Pending &&
                invitation.ExpiresAt > utcNow &&
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
