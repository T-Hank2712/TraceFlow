using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace TraceFlow.Api.Application.Common.Security;

public class ApiKeyGenerator
{
    private const string Prefix = "tfk_live";
    public GeneratedApiKey Generate(Ulid apiKeyId)
    {
        var secretBytes = RandomNumberGenerator.GetBytes(32);
        var randomSecret = WebEncoders.Base64UrlEncode(secretBytes);

        var secret = $"{Prefix}_{apiKeyId}_{randomSecret}";
        var keyPrefix = $"{Prefix}_{apiKeyId}_{randomSecret[..8]}";

        return new GeneratedApiKey(secret, keyPrefix);
    }
}

public sealed record GeneratedApiKey(
    string Secret,
    string KeyPrefix);