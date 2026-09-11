using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.ApiKeys.Commands.RevokeApiKey;

public class RevokeApiKeyCommandHandler
    : IRequestHandler<RevokeApiKeyCommand, RevokeApiKeyResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly ProjectAccessService _projectAccess;

    public RevokeApiKeyCommandHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess)
    {
        _dbContext = dbContext;
        _projectAccess = projectAccess;
    }

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

        apiKey.Revoke();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RevokeApiKeyResponse(
            apiKey.Id,
            apiKey.Name,
            apiKey.Environment,
            apiKey.KeyPrefix,
            apiKey.Status,
            apiKey.RevokedAt,
            apiKey.UpdatedAt);
    }
}