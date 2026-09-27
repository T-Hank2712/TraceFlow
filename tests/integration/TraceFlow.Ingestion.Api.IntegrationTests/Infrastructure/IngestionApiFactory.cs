using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

public sealed class IngestionApiFactory : WebApplicationFactory<Program>
{
    private readonly KafkaFixture _kafka;
    private readonly RedisFixture _redis;

    public IngestionApiFactory(
        KafkaFixture kafka,
        RedisFixture redis)
    {
        _kafka = kafka;
        _redis = redis;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Integration");

        Environment.SetEnvironmentVariable(
            "Kafka__BootstrapServers",
            _kafka.BootstrapServers);

        Environment.SetEnvironmentVariable(
            "Redis__ConnectionString",
            _redis.ConnectionString);
    }
}