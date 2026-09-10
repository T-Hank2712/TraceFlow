using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.TraceApplications.Queries.GetApplicationDetail;

public class GetApplicationDetailQueryHandler
    : IRequestHandler<GetApplicationDetailQuery, ApplicationDetailResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly ProjectAccessService _projectAccess;

    public GetApplicationDetailQueryHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess)
    {
        _dbContext = dbContext;
        _projectAccess = projectAccess;
    }

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
