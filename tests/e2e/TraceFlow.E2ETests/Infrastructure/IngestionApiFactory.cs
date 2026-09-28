using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TraceFlow.Ingestion.Api.Clients;

namespace TraceFlow.E2ETests.Infrastructure;

public sealed class IngestionApiFactory : WebApplicationFactory<Program>
{
    private readonly KafkaFixture _kafka;
    private readonly RedisFixture _redis;

    public FakeApiKeyValidator ApiKeyValidator { get; } = new();

    public IngestionApiFactory(
        KafkaFixture kafka,
        RedisFixture redis)
    {
        _kafka = kafka;
        _redis = redis;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("E2E");

        Environment.SetEnvironmentVariable("ControlApi__BaseUrl", "http://control-api.e2e");
        Environment.SetEnvironmentVariable("ControlApi__ValidateApiPath", "/internal/api-keys/validate");
        Environment.SetEnvironmentVariable("ControlApi__InternalServiceSecret", "e2e-secret");
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
        Environment.SetEnvironmentVariable("Redis__TenantContextCacheKeyPrefix", "traceflow:e2e:apikey");

        Environment.SetEnvironmentVariable("RateLimiting__PermitLimit", "100");
        Environment.SetEnvironmentVariable("RateLimiting__WindowSeconds", "60");
        Environment.SetEnvironmentVariable("RateLimiting__RateLimitKeyPrefix", "traceflow:e2e:ratelimit");

        Environment.SetEnvironmentVariable("Ingestion__MaxBatchSize", "10");
        Environment.SetEnvironmentVariable("Ingestion__MaxRequestBodyBytes", "4096");
        Environment.SetEnvironmentVariable("Ingestion__MaxMessageLength", "1024");
        Environment.SetEnvironmentVariable("Ingestion__MaxServiceLength", "128");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IApiKeyValidator>();
            services.AddSingleton<IApiKeyValidator>(ApiKeyValidator);
        });
    }
}