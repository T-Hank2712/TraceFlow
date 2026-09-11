using System.Security.Cryptography;
using System.Text;

namespace TraceFlow.Api.Application.Common.Security;

public class ApiKeyHasher
{
    private readonly IConfiguration _configuration;

    public ApiKeyHasher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Hash(string secret)
    {
        var pepper = _configuration["API_KEY_PEPPER"]
            ?? throw new InvalidOperationException("API key pepper is not configured.");

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