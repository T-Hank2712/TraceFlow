namespace TraceFlow.Api.Application.ApiKeys.Commands.RevokeApiKey;

public class RevokeApiKeyCommandHandler(
    AppDbContext dbContext,
    ProjectAccessService projectAccess,
    ApiKeyAccessService apiKeyAccess,
    IApiKeyRevocationStore apiKeyRevocationStore,
    TimeProvider timeProvider
)
    : IRequestHandler<RevokeApiKeyCommand, RevokeApiKeyResponse>
{

    private readonly AppDbContext _dbContext = dbContext;
    private readonly ProjectAccessService _projectAccess = projectAccess;
    private readonly ApiKeyAccessService _apiKeyAccess = apiKeyAccess;
    private readonly IApiKeyRevocationStore _apiKeyRevocationStore = apiKeyRevocationStore;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<RevokeApiKeyResponse> Handle(
        RevokeApiKeyCommand request,
        CancellationToken cancellationToken)
    {
        var access = await _apiKeyAccess.GetApiKeyAccessAsync(
            request.ApiKeyId,
            request.UserId,
            "API key not found.",
            cancellationToken);

        _projectAccess.EnsureProjectManager(
            access.ApplicationAccess.ProjectAccess,
            "You do not have permission to revoke API keys.");

        var apiKey = access.ApiKey;

        if (apiKey is null)
        {
            throw new NotFoundException("API key not found.");
        }

        var utcNow = _timeProvider.GetUtcNow();

        await _apiKeyRevocationStore.RevokeAsync(apiKey.Id, cancellationToken);

        apiKey.Revoke(utcNow);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RevokeApiKeyResponse(
            apiKey.Id,
            apiKey.Name,
            apiKey.Environment,
            apiKey.KeyPrefix,
            apiKey.Status,
            apiKey.RevokedAt!.Value,
            apiKey.UpdatedAt);
    }
}
