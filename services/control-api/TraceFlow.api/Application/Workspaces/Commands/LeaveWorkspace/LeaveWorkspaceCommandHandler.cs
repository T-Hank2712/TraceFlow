namespace TraceFlow.Api.Application.Workspaces.Commands.LeaveWorkspace;

public class LeaveWorkspaceCommandHandler(
        AppDbContext dbContext,
        WorkspaceAccessService workspaceAccess,
        TimeProvider timeProvider)
    : IRequestHandler<LeaveWorkspaceCommand, LeaveWorkspaceResponse>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly WorkspaceAccessService _workspaceAccess = workspaceAccess;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<LeaveWorkspaceResponse> Handle(
        LeaveWorkspaceCommand request,
        CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var membership = await _workspaceAccess.GetActiveMembershipAsync(
                request.WorkspaceId,
                request.UserId,
                "Workspace not found.",
                cancellationToken);

            _workspaceAccess.EnsureWorkspaceIsActive(
                membership.Workspace,
                "Archived workspace cannot be left.");

            if (membership.UserId == membership.Workspace.OwnerUserId)
            {
                throw new ConflictException(
                    "Workspace primary owner cannot leave. Transfer primary ownership first.");
            }

            var utcNow = _timeProvider.GetUtcNow();

            var projectMemberships = await _dbContext.ProjectMembers
                .Where(projectMember =>
                    projectMember.UserId == request.UserId &&
                    projectMember.Project.WorkspaceId == request.WorkspaceId)
                .ToListAsync(cancellationToken);

            foreach (var projectMembership in projectMemberships)
            {
                projectMembership.Remove(utcNow);
            }

            membership.Remove(utcNow);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new LeaveWorkspaceResponse(
                request.WorkspaceId,
                request.UserId,
                "Left workspace successfully.");
        });
    }
}