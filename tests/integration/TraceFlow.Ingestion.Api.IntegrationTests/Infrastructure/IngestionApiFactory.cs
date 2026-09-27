using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

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

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    // Kafka endpoint được lấy động từ Testcontainer.
                    ["Kafka:BootstrapServers"] =
                        _kafka.BootstrapServers,

                    // Redis endpoint được lấy động từ Testcontainer.
                    ["Redis:ConnectionString"] =
                        _redis.ConnectionString
                });
        });
    }
}