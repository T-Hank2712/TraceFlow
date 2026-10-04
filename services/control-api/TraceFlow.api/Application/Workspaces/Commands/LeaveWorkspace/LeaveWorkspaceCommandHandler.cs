namespace TraceFlow.Api.Application.Workspaces.Commands.LeaveWorkspace;

public class LeaveWorkspaceCommandHandler(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccess)
    : IRequestHandler<LeaveWorkspaceCommand, LeaveWorkspaceResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly WorkspaceAccessService _workspaceAccess = workspaceAccess;

 public async Task<LeaveWorkspaceResponse> Handle(
        LeaveWorkspaceCommand request,
        CancellationToken cancellationToken)
    {
        var membership = await _workspaceAccess.GetActiveMembershipAsync(
            request.WorkspaceId,
            request.UserId,
            "Workspace not found.",
            cancellationToken);

        _workspaceAccess.EnsureWorkspaceIsActive(
            membership.Workspace,
            "Archived workspace cannot be left.");

        if (membership.Role == WorkspaceMemberRoles.Owner)
        {
            var ownerCount = await _dbContext.WorkspaceMembers
                .CountAsync(
                    member =>
                        member.WorkspaceId == request.WorkspaceId &&
                        member.Role == WorkspaceMemberRoles.Owner &&
                        member.Status == MembershipStatuses.Active,
                    cancellationToken);

            if (ownerCount <= 1)
            {
                throw new ConflictException("Workspace must have at least one owner.");
            }
        }

        await _dbContext.ProjectMembers
            .Where(projectMember =>
                projectMember.UserId == request.UserId &&
                projectMember.Project.WorkspaceId == request.WorkspaceId)
            .ExecuteDeleteAsync(cancellationToken);

        _dbContext.WorkspaceMembers.Remove(membership);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LeaveWorkspaceResponse(
            request.WorkspaceId,
            request.UserId,
            "Left workspace successfully.");
    }
}
