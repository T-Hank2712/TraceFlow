using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Security;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.ApiKeys.Commands.ValidateApiKey;

public class ValidateApiKeyCommandHandler
    : IRequestHandler<ValidateApiKeyCommand, ValidateApiKeyResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly ApiKeyParser _apiKeyParser;
    private readonly ApiKeyHasher _apiKeyHasher;

    public ValidateApiKeyCommandHandler(
        AppDbContext dbContext,
        ApiKeyParser apiKeyParser,
        ApiKeyHasher apiKeyHasher)
    {
        _dbContext = dbContext;
        _apiKeyParser = apiKeyParser;
        _apiKeyHasher = apiKeyHasher;
    }

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

        var utcNow = DateTime.UtcNow;

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

        apiKey.MarkUsed();

        await _dbContext.SaveChangesAsync(cancellationToken);

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