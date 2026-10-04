namespace TraceFlow.Api.Infrastructure.DependencyInjection;

public static class ForwardedHeadersExtensions
{
    public static IServiceCollection AddForwardedClientHeaders(
        this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto;

            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }
}