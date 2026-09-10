using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Commands.UpdateProject;

public class UpdateProjectCommandHandler
    : IRequestHandler<UpdateProjectCommand, UpdateProjectResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly ProjectAccessService _projectAccess;

    public UpdateProjectCommandHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess)
    {
        _dbContext = dbContext;
        _projectAccess = projectAccess;
    }

    public async Task<UpdateProjectResponse> Handle(
        UpdateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var access = await _projectAccess.GetProjectAccessAsync(
            request.WorkspaceId,
            request.ProjectId,
            request.UserId,
            "Project not found.",
            cancellationToken);

        _projectAccess.EnsureProjectManager(
            access,
            "You do not have permission to update this project.");

        var project = access.Project;

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var normalizedSlug = request.Slug.Trim().ToLowerInvariant();

            var slugExists = await _dbContext.Projects
                .AnyAsync(
                    existingProject =>
                        existingProject.WorkspaceId == request.WorkspaceId &&
                        existingProject.Slug == normalizedSlug &&
                        existingProject.Status == ResourceStatuses.Active &&
                        existingProject.Id != request.ProjectId,
                    cancellationToken);

            if (slugExists)
            {
                throw new ConflictException("Project slug is already taken.");
            }
        }

        project.UpdateProject(
            request.Name,
            request.Slug,
            request.Description);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateProjectResponse(
            project.Id,
            project.WorkspaceId,
            project.CreatedByUserId,
            project.Name,
            project.Slug,
            project.Description,
            project.Status,
            project.UpdatedAt);
    }
}
