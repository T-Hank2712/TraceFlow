namespace TraceFlow.Api.Infrastructure.RequestLimits;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequestBodyLimitPolicyAttribute(string policyName) : Attribute, IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(
        ResourceExecutingContext context,
        ResourceExecutionDelegate next)
    {
        var options = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<RequestBodyLimitOptions>>()
            .Value;

        var limitBytes = policyName switch
        {
            RequestBodyLimitPolicies.Auth => options.AuthBytes,
            RequestBodyLimitPolicies.Internal => options.InternalBytes,
            RequestBodyLimitPolicies.Search => options.SearchBytes,
            RequestBodyLimitPolicies.Default => options.DefaultBytes,
            _ => throw new InvalidOperationException($"Unknown request body limit policy: {policyName}")
        };

        var maxRequestBodySizeFeature =
            context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();

        if (maxRequestBodySizeFeature is not null &&
            !maxRequestBodySizeFeature.IsReadOnly)
        {
            maxRequestBodySizeFeature.MaxRequestBodySize = limitBytes;
        }

        await next();
    }
}