namespace TraceFlow.Api.Application.Common.Invitations;

public class InvitationExpirationService(
    AppDbContext dbContext,
    TimeProvider timeProvider)
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task ExpireWorkspaceInvitationsForUserAsync(
        Ulid userId,
        CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow();

        await _dbContext.WorkspaceInvitations
            .Where(invitation =>
                invitation.InvitedUserId == userId &&
                invitation.Status == InvitationStatuses.Pending &&
                invitation.ExpiresAt <= utcNow)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(invitation => invitation.Status, InvitationStatuses.Expired)
                    .SetProperty(invitation => invitation.UpdatedAt, utcNow),
                cancellationToken);
    }

    public async Task ExpireWorkspaceInvitationsAsync(
        Ulid workspaceId,
        CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow();

        await _dbContext.WorkspaceInvitations
            .Where(invitation =>
                invitation.WorkspaceId == workspaceId &&
                invitation.Status == InvitationStatuses.Pending &&
                invitation.ExpiresAt <= utcNow)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(invitation => invitation.Status, InvitationStatuses.Expired)
                    .SetProperty(invitation => invitation.UpdatedAt, utcNow),
                cancellationToken);
    }

    public async Task ExpireProjectInvitationsForUserAsync(
        Ulid userId,
        CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow();

        await _dbContext.ProjectInvitations
            .Where(invitation =>
                invitation.InvitedUserId == userId &&
                invitation.Status == InvitationStatuses.Pending &&
                invitation.ExpiresAt <= utcNow)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(invitation => invitation.Status, InvitationStatuses.Expired)
                    .SetProperty(invitation => invitation.UpdatedAt, utcNow),
                cancellationToken);
    }

    public async Task ExpireProjectInvitationsAsync(
        Ulid projectId,
        CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow();

        await _dbContext.ProjectInvitations
            .Where(invitation =>
                invitation.ProjectId == projectId &&
                invitation.Status == InvitationStatuses.Pending &&
                invitation.ExpiresAt <= utcNow)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(invitation => invitation.Status, InvitationStatuses.Expired)
                    .SetProperty(invitation => invitation.UpdatedAt, utcNow),
                cancellationToken);
    }
}