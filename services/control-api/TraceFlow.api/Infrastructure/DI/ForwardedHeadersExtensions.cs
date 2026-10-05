namespace TraceFlow.Api.Infrastructure.DependencyInjection;

public static class ForwardedHeadersExtensions
{
    public static IServiceCollection AddForwardedClientHeaders(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var trustedProxyOptions = configuration
            .GetSection(TrustedProxyOptions.SectionName)
            .Get<TrustedProxyOptions>() ?? new TrustedProxyOptions();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto;

            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();

            foreach (var network in trustedProxyOptions.KnownNetworks.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                if (!System.Net.IPNetwork.TryParse(network, out var ipNetwork))
                {
                    throw new InvalidOperationException(
                        $"Invalid trusted proxy network: {network}. Use CIDR format, for example 172.20.0.0/16.");
                }

                options.KnownIPNetworks.Add(ipNetwork);
            }

            if (environment.IsProduction() &&
                options.KnownProxies.Count == 0 &&
                options.KnownIPNetworks.Count == 0)
            {
                throw new InvalidOperationException(
                    "At least one trusted network must be configured in production.");
            }
        });

        return services;
    }
}