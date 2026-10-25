namespace TraceFlow.Api.Application.TraceApplications.Commands.DeleteTraceApplication;

public class DeleteApplicationCommandHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess,
        TraceApplicationAccessService applicationAccess,
        ILogSearchReader logSearchReader,
        TimeProvider timeProvider)
    : IRequestHandler<DeleteApplicationCommand, DeleteApplicationResponse>
{

    private readonly AppDbContext _dbContext = dbContext;
    private readonly ProjectAccessService _projectAccess = projectAccess;
    private readonly TraceApplicationAccessService _applicationAccess = applicationAccess;
    private readonly ILogSearchReader _logSearchReader = logSearchReader;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<DeleteApplicationResponse> Handle(
           DeleteApplicationCommand request,
           CancellationToken cancellationToken)
    {
        var access = await _applicationAccess.GetApplicationAccessAsync(
            request.ApplicationId,
            request.UserId,
            "Application not found.",
            cancellationToken);

        _projectAccess.EnsureProjectDeveloperOrManager(
            access.ProjectAccess,
            "You do not have permission to delete this application.");

        var application = access.Application;

        if (application is null)
        {
            throw new NotFoundException("Application not found.");
        }

        var hasApiKeys = await _dbContext.ApiKeys
            .AnyAsync(
                apiKey => apiKey.TraceApplicationId == application.Id,
                cancellationToken);

        bool hasLogs = false;

        if (!hasApiKeys)
        {
            try
            {
                hasLogs = await _logSearchReader.HasLogsAsync(
                    access.ProjectAccess.Project.WorkspaceId,
                    application.ProjectId,
                    request.ApplicationId,
                    cancellationToken);
            }
            catch (ExternalServiceException)
            {
                hasLogs = true;
            }
        }

        var canHardDelete = !hasApiKeys && !hasLogs;

        if (canHardDelete)
        {
            _dbContext.TraceApplications.Remove(application);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new DeleteApplicationResponse(
                application.Id,
                application.ProjectId,
                DeleteMode.Hard,
                "Trace application permanently deleted.");
        }

        application.Archive(_timeProvider.GetUtcNow());

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteApplicationResponse(
            application.Id,
            application.ProjectId,
            DeleteMode.Soft,
            "Trace application archived.");
    }
}
