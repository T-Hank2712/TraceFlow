using System.Security.Cryptography;

namespace TraceFlow.Api.Application.Common.Security;

public class ApiKeyGenerator
{
    public GeneratedApiKey Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var secretPart = Convert.ToBase64String(bytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");

        var secret = $"tf_live_{secretPart}";
        var prefix = secret[..16];

        return new GeneratedApiKey(secret, prefix);
    }
}

public sealed record GeneratedApiKey(string Secret, string Prefix);