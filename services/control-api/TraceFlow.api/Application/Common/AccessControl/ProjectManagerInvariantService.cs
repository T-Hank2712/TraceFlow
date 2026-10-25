namespace TraceFlow.Api.Application.Common.AccessControl;

public sealed class ProjectManagerInvariantService(AppDbContext dbContext)
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task EnsureCanRemoveUserFromActiveProjectsAsync(
        Ulid workspaceId,
        Ulid userId,
        CancellationToken cancellationToken)
    {
        var managedProjectIds = await _dbContext.ProjectMembers
            .AsNoTracking()
            .Where(member =>
                member.UserId == userId &&
                member.Role == ProjectMemberRoles.Manager &&
                member.Status == MembershipStatuses.Active &&
                member.Project.WorkspaceId == workspaceId &&
                member.Project.Status == ResourceStatuses.Active)
            .Select(member => member.ProjectId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (managedProjectIds.Count == 0)
        {
            return;
        }

        var projectsWithoutAnotherManager = await _dbContext.ProjectMembers
            .AsNoTracking()
            .Where(member =>
                managedProjectIds.Contains(member.ProjectId) &&
                member.Role == ProjectMemberRoles.Manager &&
                member.Status == MembershipStatuses.Active)
            .GroupBy(member => member.ProjectId)
            .Where(group => group.Count() <= 1)
            .Select(group => group.Key)
            .ToListAsync(cancellationToken);

        if (projectsWithoutAnotherManager.Count > 0)
        {
            throw new ConflictException(
                "Cannot remove this workspace member because they are the only manager of one or more projects. Assign another project manager first.");
        }
    }
}
