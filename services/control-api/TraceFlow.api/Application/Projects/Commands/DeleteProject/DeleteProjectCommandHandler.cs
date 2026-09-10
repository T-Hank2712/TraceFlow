using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Projects.Commands.DeleteProject;

public class DeleteProjectCommandHandler
    : IRequestHandler<DeleteProjectCommand, DeleteProjectResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly ProjectAccessService _projectAccess;

    public DeleteProjectCommandHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess)
    {
        _dbContext = dbContext;
        _projectAccess = projectAccess;
    }

    public async Task<DeleteProjectResponse> Handle(
        DeleteProjectCommand request,
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
            "You do not have permission to delete this project.");

        var project = access.Project;

        var hasBusinessDependencies = false;

        if (!hasBusinessDependencies)
        {
            _dbContext.Projects.Remove(project);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new DeleteProjectResponse(
                project.Id,
                project.WorkspaceId,
                DeleteMode.Hard,
                "Project permanently deleted.");
        }

        project.Archive();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteProjectResponse(
            project.Id,
            project.WorkspaceId,
            DeleteMode.Soft,
            "Project archived.");
    }
}
