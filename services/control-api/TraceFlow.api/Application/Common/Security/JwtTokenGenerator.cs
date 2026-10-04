namespace TraceFlow.Api.Application.Common.Security;

public class JwtTokenGenerator(
    IOptions<JwtOptions> options,
    TimeProvider timeProvider)
{

    private readonly JwtOptions _options = options.Value;
    private readonly TimeProvider _timeProvider = timeProvider;


    public (string Token, DateTimeOffset ExpiresAt) Generate(User user)
    {
        var expiresAt = _timeProvider.GetUtcNow().AddMinutes(_options.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("username", user.UserName),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.Secret));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}
