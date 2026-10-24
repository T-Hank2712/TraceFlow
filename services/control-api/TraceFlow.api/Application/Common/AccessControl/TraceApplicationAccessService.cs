namespace TraceFlow.Api.Application.Common.AccessControl;

public class TraceApplicationAccessService(
    AppDbContext dbContext,
    ProjectAccessService projectAccess)
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ProjectAccessService _projectAccess = projectAccess;
    public async Task<TraceApplicationAccessContext> GetApplicationAccessAsync(
        Ulid applicationId,
        Ulid userId,
        string notFoundMessage,
        CancellationToken cancellationToken,
        string archivedWorkspaceMessage = "Archived workspace cannot be modified."
    )
    {
        var application = await _dbContext.TraceApplications
            .FirstOrDefaultAsync(
                application => application.Id == applicationId && application.Status == ResourceStatuses.Active,
                cancellationToken
            );

        if (application is null)
        {
            throw new NotFoundException(notFoundMessage);
        }

        var projectAccessContext = await _projectAccess.GetProjectAccessAsync(
            application.ProjectId,
            userId,
            notFoundMessage,
            cancellationToken,
            archivedWorkspaceMessage);

        return new TraceApplicationAccessContext(
            projectAccessContext,
            application);
    }
}

public sealed record TraceApplicationAccessContext(
    ProjectAccessContext ProjectAccess,
    TraceApplication Application);