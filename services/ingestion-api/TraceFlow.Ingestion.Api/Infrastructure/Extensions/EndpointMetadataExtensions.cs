namespace TraceFlow.Ingestion.Api.Infrastructure.Extensions;

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