namespace TraceFlow.Ingestion.Api.Endpoints;

public static class LogIngestionEndpoints
{
    public static IEndpointRouteBuilder MapLogIngestionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/logs", async (
            IngestLogRequest request,
            HttpContext httpContext,
            IIngestLogService service,
            CancellationToken cancellationToken) =>
        {
            var authentication =
                AuthenticationContext.Get(httpContext);

            if (authentication is null)
            {
                return Results.StatusCode(
                    StatusCodes.Status500InternalServerError);
            }

            var result = await service.IngestAsync(
                request,
                authentication,
                cancellationToken);

            return result.Success
                ? Results.Json(result.Data, statusCode: result.StatusCode)
                : Results.Json(result.Error, statusCode: result.StatusCode);
        }).RequireRateLimiting(RateLimitingExtensions.PreAuthPolicy);

        return app;
    }
    public static IEndpointRouteBuilder MapBatchLogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/batch-logs", async (
            BatchLogRequest request,
            HttpContext httpContext,
            IIngestLogService service,
            CancellationToken cancellationToken) =>
        {
            var authentication = AuthenticationContext.Get(httpContext);

            if (authentication is null)
            {
                return Results.StatusCode(
                    StatusCodes.Status500InternalServerError);
            }

            var result = await service.BatchLogAsync(
                request,
                authentication,
                cancellationToken);

            return result.Success
                ? Results.Json(result.Data, statusCode: result.StatusCode)
                : Results.Json(result.Error, statusCode: result.StatusCode);
        }).RequireRateLimiting(RateLimitingExtensions.PreAuthPolicy);

        return app;
    }
}
