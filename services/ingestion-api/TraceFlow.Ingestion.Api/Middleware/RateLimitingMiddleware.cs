namespace TraceFlow.Ingestion.Api.Middleware;

public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimiter _rateLimiter;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IRateLimiter rateLimiter,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _rateLimiter = rateLimiter;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.AllowsAnonymous())
        {
            await _next(context);
            return;
        }

        var authenticationContext =
            AuthenticationContext.Get(context);

        if (authenticationContext is null)
        {
            _logger.LogError(
                "Authentication context was not found before rate limiting.");

            context.Response.StatusCode =
                StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(
                new ErrorResponse(
                    ErrorCodes.InternalServerError,
                    "Internal server error."),
                context.RequestAborted);

            return;
        }

        RateLimitResult result;

        try
        {
            result = await _rateLimiter.CheckAsync(
                authenticationContext.ApiKey,
                context.RequestAborted);
        }
        catch (RedisException ex)
        {
            _logger.LogError(
                ex,
                "Redis unavailable while checking ingestion rate limit.");

            context.Response.StatusCode =
                StatusCodes.Status503ServiceUnavailable;

            await context.Response.WriteAsJsonAsync(
                new ErrorResponse(
                    ErrorCodes.RateLimitUnavailable,
                    "Rate limit service is unavailable."),
                context.RequestAborted);

            return;
        }

        if (!result.Allowed)
        {
            context.Response.StatusCode =
                StatusCodes.Status429TooManyRequests;

            context.Response.Headers.RetryAfter =
                result.RetryAfterSeconds.ToString();

            await context.Response.WriteAsJsonAsync(
                new ErrorResponse(
                    ErrorCodes.RateLimitExceeded,
                    "Too many requests.",
                    new Dictionary<string, string[]>
                    {
                        ["limit"] = [result.Limit.ToString()],
                        ["remaining"] = [result.Remaining.ToString()],
                        ["retryAfterSeconds"] = [result.RetryAfterSeconds.ToString()]
                    }),
                context.RequestAborted);

            return;
        }

        context.Response.Headers["X-RateLimit-Limit"] =
            result.Limit.ToString();

        context.Response.Headers["X-RateLimit-Remaining"] =
            result.Remaining.ToString();

        await _next(context);
    }
}
