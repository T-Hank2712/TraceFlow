namespace TraceFlow.Api.Application.Workspaces.Commands.CancelInvitation;

public class CancelInvitationCommandHandler(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccess,
        TimeProvider timeProvider
        )
    : IRequestHandler<CancelInvitationCommand, CancelInvitationResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly WorkspaceAccessService _workspaceAccess = workspaceAccess;
    private readonly TimeProvider _timeProvider = timeProvider;

 public async Task<CancelInvitationResponse> Handle(
        CancelInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var actorMembership = await _workspaceAccess.GetActiveMembershipAsync(
            request.WorkspaceId,
            request.ActorUserId,
            "Workspace invitation not found.",
            cancellationToken);

        _workspaceAccess.EnsureWorkspaceIsActive(
            actorMembership.Workspace,
            "Archived workspace cannot be modified.");

        _workspaceAccess.EnsureWorkspaceManager(
            actorMembership,
            "You do not have permission to cancel workspace invitations.");

        var invitation = await _dbContext.WorkspaceInvitations
            .FirstOrDefaultAsync(
                invitation =>
                    invitation.Id == request.InvitationId &&
                    invitation.WorkspaceId == request.WorkspaceId,
                cancellationToken);

        if (invitation is null)
        {
            throw new NotFoundException("Workspace invitation not found.");
        }

        if (invitation.Status != InvitationStatuses.Pending)
        {
            throw new ConflictException("Only pending invitation can be cancelled.");
        }

        invitation.Cancel(_timeProvider.GetUtcNow());

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CancelInvitationResponse(
            invitation.Id,
            invitation.WorkspaceId,
            invitation.Status,
            invitation.UpdatedAt);
    }
}
