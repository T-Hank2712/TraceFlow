using FluentAssertions;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

[Collection("Ingestion API Integration")]
public sealed class ContainerFixtureTests
{
    private readonly KafkaFixture _kafka;
    private readonly RedisFixture _redis;

    public ContainerFixtureTests(
        KafkaFixture kafka,
        RedisFixture redis)
    {
        _kafka = kafka;
        _redis = redis;
    }

    [Fact]
    public void Kafka_Should_Expose_Bootstrap_Server()
    {
        _kafka.BootstrapServers
            .Should()
            .NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Redis_Should_Expose_Connection_String()
    {
        _redis.ConnectionString
            .Should()
            .NotBeNullOrWhiteSpace();
    }
}