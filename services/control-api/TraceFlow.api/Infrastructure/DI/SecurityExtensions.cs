namespace TraceFlow.Api.Infrastructure.DependencyInjection;

public static class SecurityExtensions
{
    public static IServiceCollection AddJwtSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions =
            configuration
                .GetSection(JwtOptions.SectionName)
                .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "JWT options are not configured.");

        services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwtOptions.Secret)),

                        ClockSkew = TimeSpan.Zero
                    };
            })
            .AddScheme<
                AuthenticationSchemeOptions,
                InternalServiceAuthenticationHandler>(
                InternalServiceAuthentication.Scheme,
                _ => { });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy(
                InternalServiceAuthentication.Policy,
                policy =>
                {
                    policy.AuthenticationSchemes.Add(
                        InternalServiceAuthentication.Scheme);

                    policy.RequireAuthenticatedUser();

                    policy.RequireClaim(
                        InternalServiceAuthentication.ServiceClaimType,
                        InternalServiceAuthentication.ServiceName);
                });
        });

        return services;
    }
}
