using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler
    : IRequestHandler<GetProjectByIdQuery, ProjectDetailResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly ProjectAccessService _projectAccess;

    public GetProjectByIdQueryHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess)
    {
        _dbContext = dbContext;
        _projectAccess = projectAccess;
    }

    public async Task<ProjectDetailResponse> Handle(
        GetProjectByIdQuery request,
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

        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(project =>
                project.Id == request.ProjectId &&
                project.WorkspaceId == request.WorkspaceId &&
                project.Status == ResourceStatuses.Active)
            .Select(project => new ProjectDetailResponse(
                project.Id,
                project.WorkspaceId,
                project.CreatedByUserId,
                project.CreatedByUser.UserName,
                project.Name,
                project.Slug,
                project.Description,
                project.Status,
                project.CreatedAt,
                project.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
        {
            throw new NotFoundException("Project not found.");
        }

        return project;
    }
}
