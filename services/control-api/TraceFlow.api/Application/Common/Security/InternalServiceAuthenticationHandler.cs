namespace TraceFlow.Api.Application.Common.Security;

public sealed class InternalServiceAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> schemeOptions,
    ILoggerFactory loggerFactory,
    UrlEncoder encoder,
    IOptionsMonitor<InternalServiceOptions> internalServiceOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(
        schemeOptions,
        loggerFactory,
        encoder)
{
    private readonly IOptionsMonitor<InternalServiceOptions>
        _internalServiceOptions = internalServiceOptions;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var expectedSecret = _internalServiceOptions.CurrentValue.Secret;

        var providedSecret = Request.Headers[
            InternalServiceAuthentication.SecretHeader]
            .ToString();

        if (string.IsNullOrWhiteSpace(expectedSecret) ||
            string.IsNullOrWhiteSpace(providedSecret) ||
            !SecretsMatch(providedSecret, expectedSecret))
        {
            return Task.FromResult(
                AuthenticateResult.Fail("Invalid internal service credentials."));
        }

        var claims = new[]
        {
            new Claim(
                InternalServiceAuthentication.ServiceClaimType,
                InternalServiceAuthentication.ServiceName)
        };

        var identity = new ClaimsIdentity(
            claims,
            InternalServiceAuthentication.Scheme);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(
            principal,
            InternalServiceAuthentication.Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private static bool SecretsMatch(
        string providedSecret,
        string expectedSecret)
    {
        var providedBytes = Encoding.UTF8.GetBytes(providedSecret);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedSecret);

        return providedBytes.Length == expectedBytes.Length &&
            CryptographicOperations.FixedTimeEquals(
                providedBytes,
                expectedBytes);
    }
}
