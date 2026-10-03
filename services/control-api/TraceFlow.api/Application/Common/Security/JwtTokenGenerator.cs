namespace TraceFlow.Api.Application.Common.Security;

public class JwtTokenGenerator(
    IConfiguration configuration,
    TimeProvider timeProvider)
{

    private readonly IConfiguration _configuration = configuration;
    private readonly TimeProvider _timeProvider = timeProvider;


    public (string Token, DateTimeOffset ExpiresAt) Generate(User user)
    {
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"]!;
        var secret = _configuration["Jwt:Secret"]!;

        var minutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "15");
        var expiresAt = _timeProvider.GetUtcNow().AddMinutes(minutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("username", user.UserName),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secret));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}
