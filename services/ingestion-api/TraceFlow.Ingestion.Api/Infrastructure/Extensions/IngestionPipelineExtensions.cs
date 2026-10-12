namespace TraceFlow.Ingestion.Api.Infrastructure.Extensions;

public static class IngestionPipelineExtensions
{
    public static WebApplication UseIngestionPipeline(
        this WebApplication app)
    {
        app.UseRateLimiter();

        app.UseRequestBodySizeLimit();
        app.UseMiddleware<MalformedPayloadMiddleware>();
        app.UseMiddleware<AuthenticationMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();

        return app;
    }
}
