namespace TraceFlow.Api.Application.TraceApplications.Commands.UpdateApplication;

public class UpdateApplicationCommandHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess,
        TraceApplicationAccessService applicationAccess,
        TimeProvider timeProvider)
    : IRequestHandler<UpdateApplicationCommand, UpdateApplicationResponse>
{

    private readonly AppDbContext _dbContext = dbContext;
    private readonly ProjectAccessService _projectAccess = projectAccess;
    private readonly TraceApplicationAccessService _applicationAccess = applicationAccess;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<UpdateApplicationResponse> Handle(
           UpdateApplicationCommand request,
           CancellationToken cancellationToken)
    {
        var access = await _applicationAccess.GetApplicationAccessAsync(
            request.ApplicationId,
            request.UserId,
            "Application not found.",
            cancellationToken);

        _projectAccess.EnsureProjectDeveloperOrManager(
            access.ProjectAccess,
            "You do not have permission to update this application.");

        var application = access.Application;

        if (application is null)
        {
            throw new NotFoundException("Application not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var normalizedSlug = request.Slug.Trim().ToLowerInvariant();

            var slugAlreadyTaken = await _dbContext.TraceApplications
                .AnyAsync(x =>
                    x.Id != application.Id &&
                    x.ProjectId == application.ProjectId &&
                    x.Slug == normalizedSlug &&
                    x.Status == ResourceStatuses.Active,
                    cancellationToken);

            if (slugAlreadyTaken)
            {
                throw new ConflictException("Application slug is already taken.");
            }
        }

        application.Update(
            request.Name,
            request.Slug,
            request.Description,
            _timeProvider.GetUtcNow());

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateApplicationResponse(
            application.Id,
            application.ProjectId,
            application.Name,
            application.Slug,
            application.Description,
            application.Status,
            application.UpdatedAt);
    }
}
