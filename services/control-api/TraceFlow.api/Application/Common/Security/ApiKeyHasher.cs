namespace TraceFlow.Api.Application.Common.Security;

public class ApiKeyHasher(IOptions<ApiKeySecurityOptions> options)
{

    ApiKeySecurityOptions _options = options.Value;


    public string Hash(string secret)
    {
        var pepper = _options.Pepper;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(pepper));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(secret));

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
    public bool Verify(string secret, string expectedHash)
    {
        var actualHash = Hash(secret);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(actualHash),
            Encoding.UTF8.GetBytes(expectedHash));
    }
}
