namespace TraceFlow.Ingestion.Api.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseRequestBodySizeLimit(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestBodySizeLimitMiddleware>();
    }
}
