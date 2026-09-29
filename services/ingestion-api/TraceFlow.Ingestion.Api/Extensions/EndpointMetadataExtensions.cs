using Microsoft.AspNetCore.Authorization;

namespace TraceFlow.Ingestion.Api.Extensions;

public static class EndpointMetadataExtensions
{
    public static bool AllowsAnonymous(this HttpContext context)
    {
        return context
            .GetEndpoint()?
            .Metadata
            .GetMetadata<IAllowAnonymous>() is not null;
    }
}