namespace TraceFlow.Api.Application.ApiKeys.Commands.ValidateApiKey;

public class ValidateApiKeyCommandHandler(
    AppDbContext dbContext,
    ApiKeyParser apiKeyParser,
    ApiKeyHasher apiKeyHasher,
    TimeProvider timeProvider
)
    : IRequestHandler<ValidateApiKeyCommand, ValidateApiKeyResponse>
{
    private static readonly TimeSpan MinimumLastUsedUpdateInterval = TimeSpan.FromMinutes(1);
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ApiKeyParser _apiKeyParser = apiKeyParser;
    private readonly ApiKeyHasher _apiKeyHasher = apiKeyHasher;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<ValidateApiKeyResponse> Handle(
        ValidateApiKeyCommand request,
        CancellationToken cancellationToken)
    {
        var parsedKey = _apiKeyParser.Parse(request.ApiKey);

        if (parsedKey is null)
        {
            return Invalid();
        }

        var apiKey = await _dbContext.ApiKeys
            .Include(key => key.TraceApplication)
                .ThenInclude(application => application.Project)
                    .ThenInclude(project => project.Workspace)
            .FirstOrDefaultAsync(
                key => key.Id == parsedKey.ApiKeyId,
                cancellationToken);

        if (apiKey is null)
        {
            return Invalid();
        }

        if (!_apiKeyHasher.Verify(request.ApiKey.Trim(), apiKey.SecretHash))
        {
            return Invalid();
        }

        var utcNow = _timeProvider.GetUtcNow();

        if (!apiKey.IsUsable(utcNow))
        {
            return Invalid();
        }

        if (apiKey.TraceApplication.Status != ResourceStatuses.Active ||
            apiKey.TraceApplication.Project.Status != ResourceStatuses.Active ||
            apiKey.TraceApplication.Project.Workspace.Status != ResourceStatuses.Active)
        {
            return Invalid();
        }

        if (apiKey.LastUsedAt is null ||
            utcNow - apiKey.LastUsedAt.Value >= MinimumLastUsedUpdateInterval)
        {
            apiKey.MarkUsed(utcNow);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new ValidateApiKeyResponse(
            true,
            apiKey.TraceApplication.Project.WorkspaceId,
            apiKey.TraceApplication.ProjectId,
            apiKey.TraceApplicationId,
            apiKey.Environment);
    }

    private static ValidateApiKeyResponse Invalid()
    {
        return new ValidateApiKeyResponse(
            false,
            null,
            null,
            null,
            null);
    }
}
