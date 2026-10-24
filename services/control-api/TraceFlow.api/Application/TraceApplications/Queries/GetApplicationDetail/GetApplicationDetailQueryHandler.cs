namespace TraceFlow.Api.Application.TraceApplications.Queries.GetApplicationDetail;

public class GetApplicationDetailQueryHandler(
        TraceApplicationAccessService applicationAccess)
    : IRequestHandler<GetApplicationDetailQuery, ApplicationDetailResponse>
{
    private readonly TraceApplicationAccessService _applicationAccess = applicationAccess;

    public async Task<ApplicationDetailResponse> Handle(
           GetApplicationDetailQuery request,
           CancellationToken cancellationToken)
    {
        var access = await _applicationAccess.GetApplicationAccessAsync(
            request.ApplicationId,
            request.UserId,
            "Application not found.",
            cancellationToken,
            "Archived workspace cannot be accessed.");

        if (!access.ProjectAccess.IsWorkspaceManager &&
            access.ProjectAccess.ProjectMembership is null)
        {
            throw new NotFoundException("Application not found.");
        }

        var application = access.Application;

        if (application is null)
        {
            throw new NotFoundException("Application not found.");
        }

        return new ApplicationDetailResponse(
            application.Id,
            access.ProjectAccess.Project.WorkspaceId,
            application.ProjectId,
            application.Name,
            application.Slug,
            application.Description,
            application.Status,
            application.CreatedAt,
            application.UpdatedAt);
    }
}
