namespace TraceFlow.Api.Infrastructure.DependencyInjection;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rateLimitOptions = configuration
            .GetSection(RateLimitOptions.SectionName)
            .Get<RateLimitOptions>()
            ?? throw new InvalidOperationException(
                "Rate limit options are not configured.");

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        GetUserOrIpPartitionKey(httpContext),
                        _ => CreateFixedWindowOptions(rateLimitOptions.Global)));

            options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetIpPartitionKey(httpContext),
                    _ => CreateFixedWindowOptions(rateLimitOptions.Auth)));

            options.AddPolicy(RateLimitPolicies.Internal, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetIpPartitionKey(httpContext),
                    _ => CreateFixedWindowOptions(rateLimitOptions.Internal)));

            options.AddPolicy(RateLimitPolicies.Search, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetUserOrIpPartitionKey(httpContext),
                    _ => CreateFixedWindowOptions(rateLimitOptions.Search)));
        });

        return services;
    }

    private static FixedWindowRateLimiterOptions CreateFixedWindowOptions(
        RateLimitPolicyOptions options)
    {
        return new FixedWindowRateLimiterOptions
        {
            PermitLimit = options.PermitLimit,
            Window = TimeSpan.FromSeconds(options.WindowSeconds),
            QueueLimit = options.QueueLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        };
    }

    private static string GetUserOrIpPartitionKey(HttpContext httpContext)
    {
        var userId = httpContext.User.GetUserId();

        return userId?.ToString()
               ?? GetIpPartitionKey(httpContext);
    }

    private static string GetIpPartitionKey(HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString()
               ?? "unknown";
    }
}