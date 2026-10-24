namespace TraceFlow.Api.Application.Common.AccessControl;

public class ApiKeyAccessService(
    AppDbContext dbContext,
    TraceApplicationAccessService applicationAccess)
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly TraceApplicationAccessService _applicationAccess = applicationAccess;

    public async Task<ApiKeyAccessContext> GetApiKeyAccessAsync(
        Ulid apiKeyId,
        Ulid userId,
        string notFoundMessage,
        CancellationToken cancellationToken,
        string archivedWorkspaceMessage = "Archived workspace cannot be modified.")
    {
        var apiKey = await _dbContext.ApiKeys
            .FirstOrDefaultAsync(
                key => key.Id == apiKeyId,
                cancellationToken);

        if (apiKey is null)
        {
            throw new NotFoundException(notFoundMessage);
        }

        var applicationAccessContext =
            await _applicationAccess.GetApplicationAccessAsync(
                apiKey.TraceApplicationId,
                userId,
                notFoundMessage,
                cancellationToken,
                archivedWorkspaceMessage);

        return new ApiKeyAccessContext(applicationAccessContext, apiKey);
    }
}

public sealed record ApiKeyAccessContext(
    TraceApplicationAccessContext ApplicationAccess,
    ApiKey ApiKey);