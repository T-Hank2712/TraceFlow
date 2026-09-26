using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using TraceFlow.Ingestion.Api.Configuration;

namespace TraceFlow.Ingestion.Api.Middleware;

public sealed class RequestBodySizeLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly long _maxRequestBodyBytes;
    private readonly ILogger<RequestBodySizeLimitMiddleware> _logger;

    public RequestBodySizeLimitMiddleware(
        RequestDelegate next,
        IOptions<IngestionOptions> options,
        ILogger<RequestBodySizeLimitMiddleware> logger)
    {
        _next = next;
        _maxRequestBodyBytes = options.Value.MaxRequestBodyBytes;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var maxRequestBodySizeFeature =
            context.Features.Get<IHttpMaxRequestBodySizeFeature>();

        if (maxRequestBodySizeFeature is not null &&
            !maxRequestBodySizeFeature.IsReadOnly)
        {
            maxRequestBodySizeFeature.MaxRequestBodySize =
                _maxRequestBodyBytes;
        }

        await _next(context);
    }
}