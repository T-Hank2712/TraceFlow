namespace TraceFlow.Api.Application.Workspaces.Commands.AcceptWorkspaceInvitation;

public class AcceptInvitationCommandHandler(
    AppDbContext dbContext,
    TimeProvider timeProvider
    )
    : IRequestHandler<AcceptInvitationCommand, AcceptInvitationResponse>
{

    private readonly AppDbContext _dbContext = dbContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<AcceptInvitationResponse> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var invitation = await _dbContext.WorkspaceInvitations
            .Include(invitation => invitation.Workspace)
            .FirstOrDefaultAsync(
                invitation =>
                    invitation.Id == request.InvitationId &&
                    invitation.InvitedUserId == request.UserId,
                cancellationToken);

        if (invitation is null)
        {
            throw new NotFoundException("Invitation not found.");
        }

        if (invitation.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be joined.");
        }

        var alreadyMember = await _dbContext.WorkspaceMembers
            .AnyAsync(
                member =>
                    member.WorkspaceId == invitation.WorkspaceId &&
                    member.UserId == request.UserId &&
                    member.Status == MembershipStatuses.Active,
                cancellationToken);

        if (alreadyMember)
        {
            throw new ConflictException("User is already a workspace member.");
        }

        var utcNow = _timeProvider.GetUtcNow();

        invitation.Accept(utcNow);

        var member = new WorkspaceMember(
            invitation.WorkspaceId,
            request.UserId,
            invitation.Role,
            utcNow);

        _dbContext.WorkspaceMembers.Add(member);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AcceptInvitationResponse(
            invitation.Id,
            invitation.WorkspaceId,
            member.Id,
            member.Role,
            invitation.Status);
    }
}
