namespace TraceFlow.Api.Application.Common.Security;

public class ApiKeyParser
{
    private const string Prefix = "tfk_live_";
    private const int UlidLength = 26;

    public ParsedApiKey? Parse(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var normalizedApiKey = apiKey.Trim();

        if (!normalizedApiKey.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return null;
        }

        var body = normalizedApiKey[Prefix.Length..];

        if (body.Length <= UlidLength || body[UlidLength] != '_')
        {
            return null;
        }

        var apiKeyIdValue = body[..UlidLength];
        var secret = body[(UlidLength + 1)..];

        if (!Ulid.TryParse(apiKeyIdValue, out var apiKeyId))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(secret))
        {
            return null;
        }

        return new ParsedApiKey(apiKeyId);
    }
}

public sealed record ParsedApiKey(Ulid ApiKeyId);
