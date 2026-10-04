namespace TraceFlow.Api.Application.Workspaces.Queries.InvitationInbox;

public class InvitationInboxQueryHandler(
    AppDbContext dbContext,
    TimeProvider timeProvider,
    InvitationExpirationService invitationExpiration)
    : IRequestHandler<InvitationInboxQuery, IReadOnlyList<InvitationInboxResponse>>
{

    private readonly AppDbContext _dbContext = dbContext;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly InvitationExpirationService _invitationExpiration = invitationExpiration;

    public async Task<IReadOnlyList<InvitationInboxResponse>> Handle(
        InvitationInboxQuery request,
        CancellationToken cancellationToken)
    {
        await _invitationExpiration.ExpireWorkspaceInvitationsForUserAsync(
            request.UserId,
            cancellationToken);

        var utcNow = _timeProvider.GetUtcNow();

        return await _dbContext.WorkspaceInvitations
            .AsNoTracking()
            .Where(invitation =>
                invitation.InvitedUserId == request.UserId &&
                invitation.Status == InvitationStatuses.Pending &&
                invitation.ExpiresAt > utcNow)
            .OrderByDescending(invitation => invitation.CreatedAt)
            .Select(invitation => new InvitationInboxResponse(
                invitation.Id,
                invitation.WorkspaceId,
                invitation.Workspace.Name,
                invitation.Workspace.Slug,
                invitation.InvitedByUserId,
                invitation.InvitedByUser.UserName,
                invitation.Role,
                invitation.Status,
                invitation.ExpiresAt,
                invitation.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
