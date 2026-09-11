using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.ApiKeys.Queries.ListApiKeys;

public class ListApiKeysQueryHandler
    : IRequestHandler<ListApiKeysQuery, IReadOnlyList<ApiKeySummaryResponse>>
{
    private readonly AppDbContext _dbContext;
    private readonly ProjectAccessService _projectAccess;

    public ListApiKeysQueryHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess)
    {
        _dbContext = dbContext;
        _projectAccess = projectAccess;
    }

    public async Task<IReadOnlyList<ApiKeySummaryResponse>> Handle(
        ListApiKeysQuery request,
        CancellationToken cancellationToken)
    {
        var access = await _projectAccess.GetProjectAccessAsync(
            request.WorkspaceId,
            request.ProjectId,
            request.UserId,
            "Trace application not found.",
            cancellationToken);

        _projectAccess.EnsureProjectMember(
            access,
            "You do not have permission to view API keys.");

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
            throw new NotFoundException("Trace application not found.");
        }

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