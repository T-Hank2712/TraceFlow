namespace TraceFlow.Ingestion.Api.Infrastructure.Extensions;

public static class ControlApiExtensions
{
    public static IServiceCollection AddControlApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection("ControlApi")
            .Get<ControlApiOptions>()
            ?? throw new InvalidOperationException("Control API options are not configured.");

        services
            .AddHttpClient<IApiKeyValidator, ControlApiClient>(client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddStandardResilienceHandler();

        return services;
    }
}
