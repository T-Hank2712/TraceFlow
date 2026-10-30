namespace TraceFlow.Api.Application.ApiKeys.Queries.ListApiKeys;

public class ListApiKeysQueryHandler(
    AppDbContext dbContext,
    ProjectAccessService projectAccess,
    TraceApplicationAccessService applicationAccess
)
    : IRequestHandler<ListApiKeysQuery, IReadOnlyList<ApiKeySummaryResponse>>
{

    private readonly AppDbContext _dbContext = dbContext;
    private readonly ProjectAccessService _projectAccess = projectAccess;
    private readonly TraceApplicationAccessService _applicationAccess = applicationAccess;

    public async Task<IReadOnlyList<ApiKeySummaryResponse>> Handle(
        ListApiKeysQuery request,
        CancellationToken cancellationToken)
    {
        var access = await _applicationAccess.GetApplicationAccessAsync(
            request.ApplicationId,
            request.UserId,
            "Trace application not found.",
            cancellationToken);

        _projectAccess.EnsureProjectMember(
            access.ProjectAccess,
            "You do not have permission to view API keys.");

        return await _dbContext.ApiKeys
            .AsNoTracking()
            .Where(apiKey => apiKey.TraceApplicationId == request.ApplicationId)
            .OrderByDescending(apiKey => apiKey.CreatedAt)
            .Select(apiKey => new ApiKeySummaryResponse(
                apiKey.Id,
                apiKey.Name,
                apiKey.Environment,
                apiKey.KeyPrefix,
                apiKey.Status,
                apiKey.ExpiresAt,
                apiKey.RevokedAt,
                apiKey.LastUsedAt,
                apiKey.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
