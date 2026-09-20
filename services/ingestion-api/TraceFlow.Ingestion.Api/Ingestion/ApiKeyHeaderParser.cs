using TraceFlow.Ingestion.Api.Errors;

namespace TraceFlow.Ingestion.Api.Ingestion;

public sealed class ApiKeyHeaderParser
{
    private const string Scheme = "ApiKey ";
    public ApiKeyParseResult Parse(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return ApiKeyParseResult.Fail(ErrorCodes.MissingApiKey, "Missing Authorization header.");
        }
        if (!authorizationHeader.StartsWith(Scheme, StringComparison.Ordinal))
        {
            return ApiKeyParseResult.Fail(ErrorCodes.InvalidApiKey, "Authorization must use ApiKey scheme.");
        }

        var secret = authorizationHeader[Scheme.Length..].Trim();

        return string.IsNullOrWhiteSpace(secret)
            ? ApiKeyParseResult.Fail(ErrorCodes.InvalidApiKeyFormat, "API key secret is empty.")
            : ApiKeyParseResult.Ok(secret);
    }
}