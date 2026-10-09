namespace TraceFlow.Ingestion.Api.Infrastructure.Extensions;

public static class IngestionServiceExtensions
{
    public static IServiceCollection AddIngestionServices(
        this IServiceCollection services)
    {
        services.AddSingleton<ApiKeyHeaderParser>();
        services.AddSingleton<EnrichedLogEventFactory>();

        services.AddScoped<Authenticator>();
        services.AddScoped<IValidator<BatchLogRequest>, BatchLogRequestValidator>();
        services.AddScoped<IValidator<IngestLogRequest>, IngestLogRequestValidator>();
        services.AddScoped<IIngestLogService, IngestLogService>();

        return services;
    }
}