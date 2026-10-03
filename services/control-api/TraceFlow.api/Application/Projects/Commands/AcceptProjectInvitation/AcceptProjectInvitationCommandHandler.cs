namespace TraceFlow.Api.Application.Projects.Commands.AcceptProjectInvitation;

public class AcceptProjectInvitationCommandHandler(
    AppDbContext dbContext,
    TimeProvider timeProvider
)
    : IRequestHandler<AcceptProjectInvitationCommand, AcceptProjectInvitationResponse>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<AcceptProjectInvitationResponse> Handle(
        AcceptProjectInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var invitation = await _dbContext.ProjectInvitations
            .Include(invitation => invitation.Workspace)
            .Include(invitation => invitation.Project)
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

        var workspaceMember = await _dbContext.WorkspaceMembers
            .FirstOrDefaultAsync(
                member =>
                    member.WorkspaceId == invitation.WorkspaceId &&
                    member.UserId == request.UserId,
                cancellationToken);

        var utcNow = _timeProvider.GetUtcNow();

        if (workspaceMember is null)
        {
            workspaceMember = new WorkspaceMember(
                invitation.WorkspaceId,
                request.UserId,
                WorkspaceMemberRoles.Member,
                utcNow);

            _dbContext.WorkspaceMembers.Add(workspaceMember);
        }
        else if (workspaceMember.Status != MembershipStatuses.Active)
        {
            workspaceMember.Activate(utcNow);
            workspaceMember.ChangeRole(WorkspaceMemberRoles.Member, utcNow);
        }

        var existingProjectMember = await _dbContext.ProjectMembers
            .FirstOrDefaultAsync(
                member =>
                    member.ProjectId == invitation.ProjectId &&
                    member.UserId == request.UserId,
                cancellationToken);

        if (existingProjectMember is not null &&
            existingProjectMember.Status == MembershipStatuses.Active)
        {
            throw new ConflictException("User is already a project member.");
        }

        ProjectMember projectMember;

        if (existingProjectMember is null)
        {
            projectMember = new ProjectMember(
                invitation.ProjectId,
                request.UserId,
                invitation.Role,
                utcNow);

            _dbContext.ProjectMembers.Add(projectMember);
        }
        else
        {
            existingProjectMember.Activate(utcNow);
            existingProjectMember.ChangeRole(invitation.Role, utcNow);
            projectMember = existingProjectMember;
        }

        invitation.Accept(utcNow);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AcceptProjectInvitationResponse(
            invitation.Id,
            invitation.WorkspaceId,
            invitation.ProjectId,
            workspaceMember.Id,
            projectMember.Id,
            projectMember.Role,
            invitation.Status);
    }
}
