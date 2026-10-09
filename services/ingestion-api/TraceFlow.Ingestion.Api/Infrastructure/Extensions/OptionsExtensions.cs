namespace TraceFlow.Ingestion.Api.Infrastructure.Extensions;

public static class OptionsExtensions
{
    public static IServiceCollection AddApplicationOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<ControlApiOptions>()
            .Bind(configuration.GetSection("ControlApi"))
            .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "ControlApi__BaseUrl is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ValidateApiPath), "ControlApi__ValidateApiPath is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.InternalServiceSecret), "ControlApi__InternalServiceSecret is required.")
            .Validate(options => options.TimeoutSeconds > 0, "ControlApi__TimeoutSeconds must be greater than 0.")
            .ValidateOnStart();

        services
            .AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection("Kafka"))
            .Validate(options => !string.IsNullOrWhiteSpace(options.BootstrapServers), "Kafka bootstrap servers are required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Topic), "Kafka topic is required.")
            .Validate(options => options.MessageSendMaxRetries >= 0, "MessageSendMaxRetries must be greater than or equal to 0.")
            .Validate(options => options.RetryBackoffMs >= 0, "RetryBackoffMs must be greater than or equal to 0.")
            .Validate(options => options.DeliveryTimeoutMs > 0, "DeliveryTimeoutMs must be greater than 0.")
            .Validate(options => options.MessageTimeoutMs > 0, "MessageTimeoutMs must be greater than 0.")
            .Validate(options => options.QueueBufferingMaxMessages > 0, "QueueBufferingMaxMessages must be greater than 0.")
            .Validate(options => options.QueueBufferingMaxKbytes > 0, "QueueBufferingMaxKbytes must be greater than 0.")
            .Validate(options => options.LingerMs >= 0, "LingerMs must be greater than or equal to 0.")
            .ValidateOnStart();

        services
            .AddOptions<IngestionOptions>()
            .Bind(configuration.GetSection("Ingestion"))
            .Validate(options => options.MaxBatchSize > 0, "Ingestion__MaxBatchSize must be greater than 0.")
            .Validate(options => options.MaxRequestBodyBytes > 0, "Ingestion__MaxRequestBodyBytes must be greater than 0.")
            .Validate(options => options.MaxMessageLength > 0, "Ingestion__MaxMessageLength must be greater than 0.")
            .Validate(options => options.MaxServiceLength > 0, "Ingestion__MaxServiceLength must be greater than 0.")
            .ValidateOnStart();

        services
            .AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Redis__ConnectionString is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.TenantContextCacheKeyPrefix), "Redis__TenantContextCacheKeyPrefix is required.")
            .Validate(options => options.TenantContextCacheTtlSeconds > 0, "Redis__TenantContextCacheTtlSeconds must be greater than 0.")
            .Validate(options => options.TenantContextMemoryCacheTtlSeconds > 0, "Redis__TenantContextMemoryCacheTtlSeconds must be greater than 0.")
            .ValidateOnStart();

        services
            .AddOptions<RateLimitOptions>()
            .Bind(configuration.GetSection(RateLimitOptions.SectionName))
            .Validate(options => options.PermitLimit > 0, "RateLimiting__PermitLimit must be greater than 0.")
            .Validate(options => options.WindowSeconds > 0, "RateLimiting__WindowSeconds must be greater than 0.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.RateLimitKeyPrefix), "RateLimiting__RateLimitKeyPrefix is required.")
            .ValidateOnStart();

        return services;
    }
}
