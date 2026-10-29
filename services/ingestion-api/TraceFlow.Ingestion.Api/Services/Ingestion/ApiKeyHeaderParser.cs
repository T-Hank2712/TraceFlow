namespace TraceFlow.Ingestion.Api.Services.Ingestion;

public sealed class ApiKeyHeaderParser
{
    private const string Scheme = "ApiKey ";
    private const string Prefix = "tfk_live_";
    private const int UlidLength = 26;

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

    public bool TryGetApiKeyId(string apiKey, out Ulid apiKeyId)
    {
        apiKeyId = default;

        if (!apiKey.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var body = apiKey[Prefix.Length..];

        if (body.Length <= UlidLength || body[UlidLength] != '_')
        {
            return false;
        }

        return Ulid.TryParse(body[..UlidLength], out apiKeyId);
    }
}
