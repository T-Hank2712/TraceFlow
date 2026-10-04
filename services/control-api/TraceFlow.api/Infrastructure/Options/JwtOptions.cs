namespace TraceFlow.Api.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string Secret { get; init; }

    public int AccessTokenExpirationMinutes { get; init; }
    public int RefreshTokenExpirationDays { get; init; }
}
