namespace TraceFlow.Api.Infrastructure.Options;

public sealed class RefreshTokenSecurityOptions
{
    public const string SectionName = "RefreshTokenSecurity";

    public required string Pepper { get; init; }
}
