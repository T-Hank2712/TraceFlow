namespace TraceFlow.Api.Application.TraceApplications.Queries.ListTraceApplications;

public class ListTraceApplicationsQueryHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess)
    : IRequestHandler<ListTraceApplicationsQuery, IReadOnlyList<TraceApplicationSummaryResponse>>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly ProjectAccessService _projectAccess = projectAccess;

 public async Task<IReadOnlyList<TraceApplicationSummaryResponse>> Handle(
        ListTraceApplicationsQuery request,
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

        return await _dbContext.TraceApplications
            .AsNoTracking()
            .Where(application =>
                application.ProjectId == request.ProjectId &&
                application.Status == ResourceStatuses.Active)
            .OrderByDescending(application => application.CreatedAt)
            .Select(application => new TraceApplicationSummaryResponse(
                application.Id,
                application.ProjectId,
                application.Name,
                application.Slug,
                application.Description,
                application.Status,
                application.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
