namespace TraceFlow.Api.Application.Workspaces.Commands.InviteWorkspaceMember;

public class InviteWorkspaceMemberCommandHandler(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccess,
        UserLookupService userLookup,
        TimeProvider timeProvider)
    : IRequestHandler<InviteWorkspaceMemberCommand, InviteWorkspaceMemberResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly WorkspaceAccessService _workspaceAccess = workspaceAccess;

    private readonly UserLookupService _userLookup = userLookup;
    private readonly TimeProvider _timeProvider = timeProvider;

public async Task<InviteWorkspaceMemberResponse> Handle(
        InviteWorkspaceMemberCommand request,
        CancellationToken cancellationToken)
    {
        var inviterMembership = await _workspaceAccess.GetActiveMembershipAsync(
            request.WorkspaceId,
            request.InvitedByUserId,
            "Workspace not found.",
            cancellationToken);

        _workspaceAccess.EnsureWorkspaceIsActive(
            inviterMembership.Workspace,
            "Archived workspace cannot be modified.");

        _workspaceAccess.EnsureWorkspaceManager(
            inviterMembership,
            "You do not have permission to invite workspace members.");

        var invitedUser = await _userLookup.GetActiveInviteTargetAsync(
            request.Identifier,
            request.InvitedByUserId,
            cancellationToken);

        var alreadyMember = await _dbContext.WorkspaceMembers
            .AnyAsync(
                member =>
                    member.WorkspaceId == request.WorkspaceId &&
                    member.UserId == invitedUser.Id &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (alreadyMember)
        {
            throw new ConflictException("User is already a workspace member.");
        }

        var utcNow = _timeProvider.GetUtcNow();

        var pendingInvitation = await _dbContext.WorkspaceInvitations
            .FirstOrDefaultAsync(
                invitation =>
                    invitation.WorkspaceId == request.WorkspaceId &&
                    invitation.InvitedUserId == invitedUser.Id &&
                    invitation.Status == InvitationStatuses.Pending,
                cancellationToken);

        if (pendingInvitation is not null)
        {
            if (pendingInvitation.ExpiresAt > utcNow)
            {
                throw new ConflictException("User already has a pending invitation.");
            }

            pendingInvitation.Expire(utcNow);
        }

        var invitation = new WorkspaceInvitation(
            request.WorkspaceId,
            invitedUser.Id,
            request.InvitedByUserId,
            request.Role,
            utcNow.AddDays(InvitationDefaults.ExpiresAfterDays),
            utcNow);

        _dbContext.WorkspaceInvitations.Add(invitation);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new InviteWorkspaceMemberResponse(
            invitation.Id,
            invitation.WorkspaceId,
            invitedUser.Id,
            invitedUser.UserName,
            invitedUser.Email,
            invitation.Role,
            invitation.Status,
            invitation.ExpiresAt);
    }
}
