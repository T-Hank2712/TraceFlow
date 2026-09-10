using System.Security.Cryptography;
using System.Text;

namespace TraceFlow.Api.Application.Common.Security;

public class ApiKeyHasher
{
    public string Hash(string secret)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}