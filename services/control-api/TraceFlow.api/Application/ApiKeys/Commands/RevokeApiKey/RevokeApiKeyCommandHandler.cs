namespace TraceFlow.Api.Application.ApiKeys.Commands.RevokeApiKey;

public class RevokeApiKeyCommandHandler(
    AppDbContext dbContext,
    ProjectAccessService projectAccess,
    TimeProvider timeProvider
)
    : IRequestHandler<RevokeApiKeyCommand, RevokeApiKeyResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly ProjectAccessService _projectAccess = projectAccess;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<RevokeApiKeyResponse> Handle(
        RevokeApiKeyCommand request,
        CancellationToken cancellationToken)
    {
        var access = await _projectAccess.GetProjectAccessAsync(
            request.WorkspaceId,
            request.ProjectId,
            request.UserId,
            "API key not found.",
            cancellationToken);

        _projectAccess.EnsureProjectManager(
            access,
            "You do not have permission to revoke API keys.");

        var applicationExists = await _dbContext.TraceApplications
            .AsNoTracking()
            .AnyAsync(
                application =>
                    application.Id == request.ApplicationId &&
                    application.ProjectId == request.ProjectId &&
                    application.Status == ResourceStatuses.Active,
                cancellationToken);

        if (!applicationExists)
        {
            throw new NotFoundException("API key not found.");
        }

        var apiKey = await _dbContext.ApiKeys
            .FirstOrDefaultAsync(
                key =>
                    key.Id == request.ApiKeyId &&
                    key.TraceApplicationId == request.ApplicationId,
                cancellationToken);

        if (apiKey is null)
        {
            throw new NotFoundException("API key not found.");
        }

        var utcNow = _timeProvider.GetUtcNow();

        apiKey.Revoke(utcNow);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RevokeApiKeyResponse(
            apiKey.Id,
            apiKey.Name,
            apiKey.Environment,
            apiKey.KeyPrefix,
            apiKey.Status,
            utcNow,
            utcNow);
    }
}
