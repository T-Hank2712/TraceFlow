using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TraceFlow.Ingestion.Api.Clients;
using TraceFlow.Ingestion.Api.Kafka;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

public sealed class IngestionApiFactory : WebApplicationFactory<Program>
{
    private readonly KafkaFixture _kafka;
    private readonly RedisFixture _redis;
    private readonly TestPublisherMode _publisherMode;

    public FakeApiKeyValidator ApiKeyValidator { get; } = new();
    public NoopLogEventPublisher LogEventPublisher { get; } = new();
    public FailingLogEventPublisher FailingLogEventPublisher { get; } = new();

    public IngestionApiFactory(
        KafkaFixture kafka,
        RedisFixture redis,
        TestPublisherMode publisherMode = TestPublisherMode.Recording)
    {
        _kafka = kafka;
        _redis = redis;
        _publisherMode = publisherMode;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Integration");

        Environment.SetEnvironmentVariable("ControlApi__BaseUrl", "http://control-api.test");
        Environment.SetEnvironmentVariable("ControlApi__ValidateApiPath", "/internal/api-keys/validate");
        Environment.SetEnvironmentVariable("ControlApi__InternalServiceSecret", "test-secret");
        Environment.SetEnvironmentVariable("ControlApi__TimeoutSeconds", "5");

        Environment.SetEnvironmentVariable("Kafka__BootstrapServers", _kafka.BootstrapServers);
        Environment.SetEnvironmentVariable("Kafka__Topic", "traceflow.logs");
        Environment.SetEnvironmentVariable("Kafka__MessageSendMaxRetries", "0");
        Environment.SetEnvironmentVariable("Kafka__RetryBackoffMs", "10");
        Environment.SetEnvironmentVariable("Kafka__DeliveryTimeoutMs", "5000");
        Environment.SetEnvironmentVariable("Kafka__MessageTimeoutMs", "5000");
        Environment.SetEnvironmentVariable("Kafka__QueueBufferingMaxMessages", "10000");
        Environment.SetEnvironmentVariable("Kafka__QueueBufferingMaxKbytes", "1048576");
        Environment.SetEnvironmentVariable("Kafka__LingerMs", "0");

        Environment.SetEnvironmentVariable("Redis__ConnectionString", _redis.ConnectionString);
        Environment.SetEnvironmentVariable("Redis__TenantContextCacheTtlSeconds", "3600");
        Environment.SetEnvironmentVariable("Redis__TenantContextCacheKeyPrefix", "traceflow:test:apikey");

        Environment.SetEnvironmentVariable("RateLimiting__PermitLimit", "100");
        Environment.SetEnvironmentVariable("RateLimiting__WindowSeconds", "60");
        Environment.SetEnvironmentVariable("RateLimiting__RateLimitKeyPrefix", "traceflow:test:ratelimit");

        Environment.SetEnvironmentVariable("Ingestion__MaxBatchSize", "3");
        Environment.SetEnvironmentVariable("Ingestion__MaxRequestBodyBytes", "4096");
        Environment.SetEnvironmentVariable("Ingestion__MaxMessageLength", "128");
        Environment.SetEnvironmentVariable("Ingestion__MaxServiceLength", "64");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IApiKeyValidator>();
            services.AddSingleton<IApiKeyValidator>(ApiKeyValidator);

            if (_publisherMode == TestPublisherMode.RealKafka)
            {
                return;
            }

            services.RemoveAll<ILogEventPublisher>();

            if (_publisherMode == TestPublisherMode.Failing)
            {
                services.AddSingleton<ILogEventPublisher>(FailingLogEventPublisher);
                return;
            }

            services.AddSingleton<ILogEventPublisher>(LogEventPublisher);
        });
    }
}