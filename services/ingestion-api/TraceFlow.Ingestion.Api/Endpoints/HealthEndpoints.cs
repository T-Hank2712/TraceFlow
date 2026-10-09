namespace TraceFlow.Ingestion.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Redirect("/health/live"))
            .AllowAnonymous();

        app.MapHealthChecks(
            "/health/live",
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live")
            })
            .AllowAnonymous();

        app.MapHealthChecks(
            "/health/ready",
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready")
            })
            .AllowAnonymous();

        return app;
    }
}
