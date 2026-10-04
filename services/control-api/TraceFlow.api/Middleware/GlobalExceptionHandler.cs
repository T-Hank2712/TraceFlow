namespace TraceFlow.Api.Middleware;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private readonly IHostEnvironment _environment = environment;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problem = CreateProblemDetails(httpContext, exception);

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred. TraceId={TraceId}",
                httpContext.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Handled exception occurred. TraceId={TraceId}",
                httpContext.TraceIdentifier);
        }

        httpContext.Response.StatusCode =
            problem.Status ?? StatusCodes.Status500InternalServerError;

        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }

    private ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        Exception exception)
    {
        var status = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            ExternalServiceException => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status500InternalServerError
        };

        var title = exception switch
        {
            ValidationException => "Validation failed.",
            UnauthorizedException => "Unauthorized.",
            ForbiddenException => "Forbidden.",
            NotFoundException => "Resource not found.",
            ConflictException => "Conflict.",
            ExternalServiceException => "External service error.",
            _ => "Unexpected server error."
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = ShouldExposeDetail(status)
                ? exception.Message
                : "An unexpected error occurred.",
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException validationException)
        {
            problem.Extensions["errors"] = validationException.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(error => error.ErrorMessage)
                        .ToArray());
        }

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (_environment.IsDevelopment())
        {
            problem.Extensions["exception"] = exception.GetType().Name;
        }

        return problem;
    }

    private bool ShouldExposeDetail(int status)
    {
        return status < StatusCodes.Status500InternalServerError ||
               _environment.IsDevelopment();
    }
}
