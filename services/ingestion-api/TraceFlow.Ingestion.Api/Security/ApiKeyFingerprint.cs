namespace TraceFlow.Ingestion.Api.Security;

public static class ApiKeyFingerprint
{
    public static string Create(string apiKey)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));

        return Convert.ToHexString(hash);
    }
}
