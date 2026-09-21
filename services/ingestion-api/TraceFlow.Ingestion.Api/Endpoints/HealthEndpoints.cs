namespace TraceFlow.Ingestion.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Redirect("/health"));

        app.MapGet("/health", () => Results.Ok(new
        {
            service = "traceflow-ingestion-api",
            status = "healthy",
            timestamp = DateTimeOffset.UtcNow
        }));

        return app;
    }
}
