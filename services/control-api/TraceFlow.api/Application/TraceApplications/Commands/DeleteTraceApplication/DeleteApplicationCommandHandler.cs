namespace TraceFlow.Api.Application.TraceApplications.Commands.DeleteTraceApplication;

public class DeleteApplicationCommandHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess,
        ILogSearchReader logSearchReader,
        TimeProvider timeProvider)
    : IRequestHandler<DeleteApplicationCommand, DeleteApplicationResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly ProjectAccessService _projectAccess = projectAccess;
    private readonly ILogSearchReader _logSearchReader = logSearchReader;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<DeleteApplicationResponse> Handle(
           DeleteApplicationCommand request,
           CancellationToken cancellationToken)
    {
        var access = await _projectAccess.GetProjectAccessAsync(
            request.WorkspaceId,
            request.ProjectId,
            request.UserId,
            "Application not found.",
            cancellationToken);

        _projectAccess.EnsureProjectDeveloperOrManager(
            access,
            "You do not have permission to delete this application.");

        var application = await _dbContext.TraceApplications
            .FirstOrDefaultAsync(
                application =>
                    application.Id == request.ApplicationId &&
                    application.ProjectId == request.ProjectId &&
                    application.Status == ResourceStatuses.Active,
                cancellationToken);

        if (application is null)
        {
            throw new NotFoundException("Application not found.");
        }

        var hasApiKeys = await _dbContext.ApiKeys
            .AnyAsync(
                apiKey => apiKey.TraceApplicationId == request.ApplicationId,
                cancellationToken);

        bool hasLogs = false;

        if (!hasApiKeys)
        {
            try
            {
                hasLogs = await _logSearchReader.HasLogsAsync(
                    request.WorkspaceId,
                    request.ProjectId,
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
