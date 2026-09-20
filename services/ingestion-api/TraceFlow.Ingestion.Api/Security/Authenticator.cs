using TraceFlow.Ingestion.Api.Ingestion;
using TraceFlow.Ingestion.Api.Contracts;
using TraceFlow.Ingestion.Api.Clients;
using TraceFlow.Ingestion.Api.Errors;

namespace TraceFlow.Ingestion.Api.Security;

public sealed class Authenticator
{
    private readonly ApiKeyHeaderParser _apiKeyParser;
    private readonly IApiKeyValidator _apiKeyValidator;
    public Authenticator(ApiKeyHeaderParser apiKeyParser, IApiKeyValidator apiKeyValidator)
    {
        _apiKeyParser = apiKeyParser;
        _apiKeyValidator = apiKeyValidator;
    }
    public async Task<Result<ApiKeyValidationResult>> AuthenticateAsync(string? authorizationHeader, CancellationToken cancellationToken)
    {
        var parsedKey = _apiKeyParser.Parse(authorizationHeader);

        if (!parsedKey.Success)
        {
            return Result<ApiKeyValidationResult>.Fail(
                parsedKey.ErrorCode!,
                parsedKey.ErrorMessage!,
                StatusCodes.Status401Unauthorized);
        }

        var tenant = await _apiKeyValidator.ValidateAsync(
            parsedKey.ApiKey!,
            cancellationToken
        );

        if (!tenant.Valid)
        {
            return Result<ApiKeyValidationResult>.Fail(
                ErrorCodes.InvalidApiKey,
                "API key is invalid, revoked, or expired.",
                StatusCodes.Status401Unauthorized);
        }
        return Result<ApiKeyValidationResult>.Ok(tenant);
    }
}
