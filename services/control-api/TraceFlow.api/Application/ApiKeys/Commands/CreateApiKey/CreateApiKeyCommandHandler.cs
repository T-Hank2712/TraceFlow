using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.AccessControl;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Application.Common.Security;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.ApiKeys.Commands.CreateApiKey;

public class CreateApiKeyCommandHandler
    : IRequestHandler<CreateApiKeyCommand, CreateApiKeyResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly ProjectAccessService _projectAccess;
    private readonly ApiKeyGenerator _apiKeyGenerator;
    private readonly ApiKeyHasher _apiKeyHasher;
    private readonly ApiKeyExpirationPolicyResolver _expirationPolicyResolver;

    public CreateApiKeyCommandHandler(
        AppDbContext dbContext,
        ProjectAccessService projectAccess,
        ApiKeyGenerator apiKeyGenerator,
        ApiKeyHasher apiKeyHasher,
        ApiKeyExpirationPolicyResolver expirationPolicyResolver)
    {
        _dbContext = dbContext;
        _projectAccess = projectAccess;
        _apiKeyGenerator = apiKeyGenerator;
        _apiKeyHasher = apiKeyHasher;
        _expirationPolicyResolver = expirationPolicyResolver;
    }

    public async Task<CreateApiKeyResponse> Handle(
        CreateApiKeyCommand request,
        CancellationToken cancellationToken)
    {
        var access = await _projectAccess.GetProjectAccessAsync(
            request.WorkspaceId,
            request.ProjectId,
            request.UserId,
            "Trace application not found.",
            cancellationToken);

        _projectAccess.EnsureProjectManager(
            access,
            "You do not have permission to create API keys.");

        var application = await _dbContext.TraceApplications
            .FirstOrDefaultAsync(
                app =>
                    app.Id == request.ApplicationId &&
                    app.ProjectId == request.ProjectId &&
                    app.Status == ResourceStatuses.Active,
                cancellationToken);

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

        var apiKey = new ApiKey(
            apiKeyId,
            request.ApplicationId,
            request.Name,
            environment,
            generatedKey.KeyPrefix,
            secretHash,
            expiresAt);

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