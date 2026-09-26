using TraceFlow.Ingestion.Api.Security;
using TraceFlow.Ingestion.Api.Services.RateLimiting;

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
        var authenticationContext =
            AuthenticationContext.Get(context);

        if (authenticationContext is null)
        {
            _logger.LogError(
                "Authentication context was not found before rate limiting.");

            context.Response.StatusCode =
                StatusCodes.Status500InternalServerError;

            return;
        }

        var result = await _rateLimiter.CheckAsync(
            authenticationContext.ApiKey,
            context.RequestAborted);

        if (!result.Allowed)
        {
            context.Response.StatusCode =
                StatusCodes.Status429TooManyRequests;

            context.Response.Headers.RetryAfter =
                result.RetryAfterSeconds.ToString();

            await context.Response.WriteAsJsonAsync(
                new
                {
                    error = "rate_limit_exceeded",
                    message = "Too many requests.",
                    limit = result.Limit,
                    remaining = result.Remaining,
                    retryAfterSeconds = result.RetryAfterSeconds
                },
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