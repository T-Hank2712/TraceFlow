namespace TraceFlow.Ingestion.Api.Infrastructure.Extensions;

public static class ControlApiExtensions
{
    public static IServiceCollection AddControlApiClient(
        this IServiceCollection services)
    {
        services.AddHttpClient<IApiKeyValidator, ControlApiClient>();

        return services;
    }
}