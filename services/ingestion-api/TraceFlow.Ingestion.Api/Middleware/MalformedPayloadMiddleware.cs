namespace TraceFlow.Ingestion.Api.Middleware;

public sealed class MalformedPayloadMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MalformedPayloadMiddleware> _logger;

    public MalformedPayloadMiddleware(
        RequestDelegate next,
        ILogger<MalformedPayloadMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BadHttpRequestException ex)
        {
            await WriteInvalidPayloadAsync(context, ex);
        }
        catch (JsonException ex)
        {
            await WriteInvalidPayloadAsync(context, ex);
        }
    }

    private async Task WriteInvalidPayloadAsync(
        HttpContext context,
        Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                exception,
                "Malformed request payload was detected after response started.");

            throw exception;
        }

        _logger.LogWarning(
            exception,
            "Malformed request payload was rejected.");

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        await context.Response.WriteAsJsonAsync(
            new ErrorResponse(
                ErrorCodes.InvalidPayload,
                "Request body is invalid or malformed."),
            context.RequestAborted);
    }
}