namespace TraceFlow.Ingestion.Api.Infrastructure.Extensions;

public static class IngestionPipelineExtensions
{
    public static WebApplication UseIngestionPipeline(
        this WebApplication app)
    {
        app.UseRateLimiter();

        app.UseMiddleware<MalformedPayloadMiddleware>();
        app.UseRequestBodySizeLimit();
        app.UseMiddleware<AuthenticationMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();

        return app;
    }
}
