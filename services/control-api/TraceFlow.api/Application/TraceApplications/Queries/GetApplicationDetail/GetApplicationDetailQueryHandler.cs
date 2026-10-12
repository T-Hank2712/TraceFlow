namespace TraceFlow.Api.Application.TraceApplications.Queries.GetApplicationDetail;

public class GetApplicationDetailQueryHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess)
    : IRequestHandler<GetApplicationDetailQuery, ApplicationDetailResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly ProjectAccessService _projectAccess = projectAccess;

    public async Task<ApplicationDetailResponse> Handle(
           GetApplicationDetailQuery request,
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

        var application = await _dbContext.TraceApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Id == request.ApplicationId &&
                x.ProjectId == request.ProjectId &&
                x.Status == ResourceStatuses.Active,
                cancellationToken);

        if (application is null)
        {
            throw new NotFoundException("Application not found.");
        }

        return new ApplicationDetailResponse(
            application.Id,
            request.WorkspaceId,
            application.ProjectId,
            application.Name,
            application.Slug,
            application.Description,
            application.Status,
            application.CreatedAt,
            application.UpdatedAt);
    }
}
