namespace TraceFlow.Api.Application.Projects.Commands.DeleteProject;

public class DeleteProjectCommandHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess,
        TimeProvider timeProvider)
    : IRequestHandler<DeleteProjectCommand, DeleteProjectResponse>
{

    private readonly AppDbContext _dbContext = dbContext;
    private readonly ProjectAccessService _projectAccess = projectAccess;
    private readonly TimeProvider _timeProvider = timeProvider;

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

        project.Archive(_timeProvider.GetUtcNow());

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteProjectResponse(
            project.Id,
            project.WorkspaceId,
            DeleteMode.Soft,
            "Project archived.");
    }
}
