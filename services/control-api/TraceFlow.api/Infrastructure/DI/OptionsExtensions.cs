namespace TraceFlow.Api.Infrastructure.DependencyInjection;

public static class OptionsExtensions
{
    public static IServiceCollection AddTypedOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Secret), "JWT secret is required.")
            .Validate(options => options.AccessTokenExpirationMinutes > 0, "JWT access token expiration must be greater than 0.")
            .Validate(options => options.RefreshTokenExpirationDays > 0, "JWT refresh token expiration must be greater than 0.")
            .ValidateOnStart();

        services
            .AddOptions<OpenSearchOptions>()
            .Bind(configuration.GetSection(OpenSearchOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Url), "OpenSearch URL is required.")
            .Validate(options => Uri.TryCreate(options.Url, UriKind.Absolute, out _), "OpenSearch URL must be absolute.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Username), "OpenSearch username is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "OpenSearch password is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Index), "OpenSearch index is required.")
            .ValidateOnStart();

        services
            .AddOptions<ApiKeySecurityOptions>()
            .Bind(configuration.GetSection(ApiKeySecurityOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Pepper), "API key pepper is required.")
            .ValidateOnStart();

        services
            .AddOptions<InternalServiceOptions>()
            .Bind(configuration.GetSection(InternalServiceOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Secret), "Internal service secret is required.")
            .ValidateOnStart();

        services
            .AddOptions<RateLimitOptions>()
            .Bind(configuration.GetSection(RateLimitOptions.SectionName))
            .Validate(options => options.Global.PermitLimit > 0, "Global rate limit permit limit must be greater than 0.")
            .Validate(options => options.Global.WindowSeconds > 0, "Global rate limit window must be greater than 0.")
            .Validate(options => options.Global.QueueLimit >= 0, "Global rate limit queue limit must not be negative.")
            .Validate(options => options.Auth.PermitLimit > 0, "Auth rate limit permit limit must be greater than 0.")
            .Validate(options => options.Auth.WindowSeconds > 0, "Auth rate limit window must be greater than 0.")
            .Validate(options => options.Auth.QueueLimit >= 0, "Auth rate limit queue limit must not be negative.")
            .Validate(options => options.Internal.PermitLimit > 0, "Internal rate limit permit limit must be greater than 0.")
            .Validate(options => options.Internal.WindowSeconds > 0, "Internal rate limit window must be greater than 0.")
            .Validate(options => options.Internal.QueueLimit >= 0, "Internal rate limit queue limit must not be negative.")
            .Validate(options => options.Search.PermitLimit > 0, "Search rate limit permit limit must be greater than 0.")
            .Validate(options => options.Search.WindowSeconds > 0, "Search rate limit window must be greater than 0.")
            .Validate(options => options.Search.QueueLimit >= 0, "Search rate limit queue limit must not be negative.")
            .ValidateOnStart();

        return services;
    }
}