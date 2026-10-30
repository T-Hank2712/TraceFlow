namespace TraceFlow.Ingestion.Api.Services.Ingestion;

public sealed record ApiKeyParseResult(
    bool Success,
    string? ApiKey,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static ApiKeyParseResult Ok(string apiKey) => new(true, apiKey, null, null);

    public static ApiKeyParseResult Fail(string code, string message) => new(false, null, code, message);
}
