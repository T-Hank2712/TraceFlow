namespace TraceFlow.Ingestion.Api.Infrastructure.Extensions;

public static class KafkaExtensions
{
    public static IServiceCollection AddKafkaProducer(
        this IServiceCollection services)
    {
        services.AddSingleton<ILogEventPublisher, KafkaLogProducer>();

        return services;
    }
}