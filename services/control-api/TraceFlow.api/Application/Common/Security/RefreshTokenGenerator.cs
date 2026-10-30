namespace TraceFlow.Api.Application.Common.Security;

public class RefreshTokenGenerator(IOptions<RefreshTokenSecurityOptions> options)
{
    private readonly RefreshTokenSecurityOptions _options = options.Value;

    public RefreshTokenResult Generate()
    {
        var token = GenerateRawToken();
        var hash = Hash(token);

        return new RefreshTokenResult(token, hash);
    }

    public string Hash(string refreshToken)
    {
        var key = Encoding.UTF8.GetBytes(_options.Pepper);
        var bytes = Encoding.UTF8.GetBytes(refreshToken);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(bytes);

        return Convert.ToHexString(hash);
    }

    private static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}

public record RefreshTokenResult(
    string Token,
    string Hash
);
