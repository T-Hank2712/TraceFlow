namespace TraceFlow.Api.Application.ApiKeys.Commands.CreateApiKey;

public class CreateApiKeyCommandHandler(
    AppDbContext dbContext,
    ProjectAccessService projectAccess,
    TraceApplicationAccessService applicationAccess,
    ApiKeyGenerator apiKeyGenerator,
    ApiKeyHasher apiKeyHasher,
    ApiKeyExpirationPolicyResolver expirationPolicyResolver,
    TimeProvider timeProvider
)
    : IRequestHandler<CreateApiKeyCommand, CreateApiKeyResponse>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ProjectAccessService _projectAccess = projectAccess;
    private readonly TraceApplicationAccessService _applicationAccess = applicationAccess;
    private readonly ApiKeyGenerator _apiKeyGenerator = apiKeyGenerator;
    private readonly ApiKeyHasher _apiKeyHasher = apiKeyHasher;
    private readonly ApiKeyExpirationPolicyResolver _expirationPolicyResolver = expirationPolicyResolver;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<CreateApiKeyResponse> Handle(
        CreateApiKeyCommand request,
        CancellationToken cancellationToken)
    {
        var access = await _applicationAccess.GetApplicationAccessAsync(
            request.ApplicationId,
            request.UserId,
            "Trace application not found.",
            cancellationToken);

        _projectAccess.EnsureProjectManager(
            access.ProjectAccess,
            "You do not have permission to create API keys.");

        var application = access.Application;

        if (application is null)
        {
            throw new NotFoundException("Trace application not found.");
        }

        var environment = request.Environment.Trim().ToLowerInvariant();
        var expirationPolicy = request.ExpirationPolicy;

        var apiKeyId = Ulid.NewUlid();
        var generatedKey = _apiKeyGenerator.Generate(apiKeyId);
        var secretHash = _apiKeyHasher.Hash(generatedKey.Secret);
        var expiresAt = _expirationPolicyResolver.Resolve(expirationPolicy);

        var utcNow = _timeProvider.GetUtcNow();

        var apiKey = new ApiKey(
            apiKeyId,
            request.ApplicationId,
            request.Name,
            environment,
            generatedKey.KeyPrefix,
            secretHash,
            expiresAt,
            utcNow,
            utcNow);

        _dbContext.ApiKeys.Add(apiKey);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateApiKeyResponse(
            apiKey.Id,
            apiKey.Name,
            apiKey.Environment,
            apiKey.KeyPrefix,
            generatedKey.Secret,
            apiKey.Status,
            apiKey.ExpiresAt,
            apiKey.CreatedAt);
    }
}
