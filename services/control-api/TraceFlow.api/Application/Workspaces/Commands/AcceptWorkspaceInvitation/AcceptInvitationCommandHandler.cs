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

        var utcNow = _timeProvider.GetUtcNow();

        if (invitation is null)
        {
            throw new NotFoundException("Invitation not found.");
        }

        if (invitation.Status != InvitationStatuses.Pending || invitation.ExpiresAt <= utcNow)
        {
            throw new ConflictException("Invitation is no longer valid or has already been processed.");
        }

        if (invitation.Workspace.Status == ResourceStatuses.Archived)
        {
            throw new ConflictException("Archived workspace cannot be joined.");
        }

        var existingMember = await _dbContext.WorkspaceMembers
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == invitation.WorkspaceId &&
                    member.UserId == request.UserId,
                cancellationToken);

        if (existingMember?.Status == MembershipStatuses.Active)
        {
            throw new ConflictException("User is already a workspace member.");
        }

        invitation.Accept(utcNow);

        WorkspaceMember member;

        if (existingMember is null)
        {
            member = new WorkspaceMember(
                invitation.WorkspaceId,
                request.UserId,
                invitation.Role,
                utcNow
            );
            _dbContext.WorkspaceMembers.Add(member);
        }
        else
        {
            existingMember.Activate(utcNow);
            existingMember.ChangeRole(invitation.Role, utcNow);

            member = existingMember;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AcceptInvitationResponse(
            invitation.Id,
            invitation.WorkspaceId,
            member.Id,
            member.Role,
            invitation.Status);
    }
}
