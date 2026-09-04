using System.Security.Cryptography;
using System.Text;

namespace TraceFlow.Api.Application.Common.Security;

public class RefreshTokenGenerator
{
    public RefreshTokenResult Generate()
    {
        var token = GenerateRawToken();
        var hash = Hash(token);

        return new RefreshTokenResult(token, hash);
    }

    public static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
    public static string Hash(string refreshToken)
    {
        var bytes = Encoding.UTF8.GetBytes(refreshToken);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}

public record RefreshTokenResult(
    string Token,
    string Hash
);