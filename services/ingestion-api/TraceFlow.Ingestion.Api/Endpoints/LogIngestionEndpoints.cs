using Microsoft.Extensions.Primitives;
using TraceFlow.Ingestion.Api.Contracts;
using TraceFlow.Ingestion.Api.Ingestion;

namespace TraceFlow.Ingestion.Api.Endpoints;

public static class LogIngestionEndpoints
{
    public static IEndpointRouteBuilder MapLogIngestionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/logs", async (
            IngestLogRequest request,
            HttpContext httpContext,
            IIngestLogService service,
            CancellationToken cancellationToken) =>
        {
            httpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorization);

            var result = await service.IngestAsync(
                request,
                authorization.ToString(),
                cancellationToken);

            return result.Success
                ? Results.Json(result.Response, statusCode: result.StatusCode)
                : Results.Json(result.Error, statusCode: result.StatusCode);
        });

        return app;
    }
}