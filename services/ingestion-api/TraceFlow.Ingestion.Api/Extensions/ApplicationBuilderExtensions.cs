using TraceFlow.Ingestion.Api.Middleware;

namespace TraceFlow.Ingestion.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseRequestBodySizeLimit(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestBodySizeLimitMiddleware>();
    }
}