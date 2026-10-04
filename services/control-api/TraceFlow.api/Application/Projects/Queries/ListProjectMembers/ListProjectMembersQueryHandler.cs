namespace TraceFlow.Api.Application.Projects.Queries.ListProjectMembers;

public class ListProjectMembersQueryHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess)
    : IRequestHandler<ListProjectMembersQuery, IReadOnlyList<ProjectMemberResponse>>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly ProjectAccessService _projectAccess = projectAccess;

    public async Task<IReadOnlyList<ProjectMemberResponse>> Handle(
           ListProjectMembersQuery request,
           CancellationToken cancellationToken)
    {
        var access = await _projectAccess.GetProjectAccessAsync(
            request.WorkspaceId,
            request.ProjectId,
            request.UserId,
            "Project not found.",
            cancellationToken,
            "Archived workspace cannot be accessed.");

        if (!access.IsWorkspaceManager && access.ProjectMembership is null)
        {
            throw new NotFoundException("Project not found.");
        }

        return await _dbContext.ProjectMembers
            .AsNoTracking()
            .Where(member =>
                member.ProjectId == request.ProjectId &&
                member.Status == MembershipStatuses.Active)
            .OrderBy(member => member.Role == ProjectMemberRoles.Manager ? 0 :
                               member.Role == ProjectMemberRoles.Developer ? 1 : 2)
            .ThenBy(member => member.User.UserName)
            .Select(member => new ProjectMemberResponse(
                member.Id,
                member.ProjectId,
                member.UserId,
                member.User.UserName,
                member.User.Email,
                member.Role,
                member.Status,
                member.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
