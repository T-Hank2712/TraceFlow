namespace TraceFlow.Ingestion.Api.Infrastructure.Extensions;

public static class RateLimitingExtensions
{
    public const string PreAuthPolicy = "pre-auth";
    public static IServiceCollection AddPreAuthRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration
            .GetSection(PreAuthRateLimitOptions.SectionName)
            .Get<PreAuthRateLimitOptions>() ?? throw new InvalidOperationException("Pre-auth rate limit options are not configured.");

        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;

            rateLimiterOptions.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode =
                    StatusCodes.Status429TooManyRequests;

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new ErrorResponse(
                        ErrorCodes.RateLimitExceeded,
                        "Too many requests."),
                    cancellationToken);
            };

            rateLimiterOptions.AddPolicy(PreAuthPolicy, httpContext =>
            {
                var partitionKey = GetClientPartitionKey(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = options.PermitLimit,
                        Window = TimeSpan.FromSeconds(options.WindowSeconds),
                        QueueLimit = options.QueueLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    });
            });
        });
        return services;
    }
    private static string GetClientPartitionKey(HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
    }
}
