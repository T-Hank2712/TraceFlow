namespace TraceFlow.Ingestion.Api.Infrastructure.Extensions;

public static class IngestionPipelineExtensions
{
    public static WebApplication UseIngestionPipeline(
        this WebApplication app)
    {
        app.UseRequestBodySizeLimit();
        app.UseMiddleware<AuthenticationMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();

        return app;
    }
}