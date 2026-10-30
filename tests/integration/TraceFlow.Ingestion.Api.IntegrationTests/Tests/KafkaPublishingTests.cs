using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TraceFlow.Ingestion.Api.Contracts.Authentication;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Tests;

[Collection("Ingestion API Integration")]
public sealed class KafkaPublishingTests
{
    private readonly KafkaFixture _kafka;
    private readonly RedisFixture _redis;

    public KafkaPublishingTests(KafkaFixture kafka, RedisFixture redis)
    {
        _kafka = kafka;
        _redis = redis;
    }

    [Fact]
    public async Task PostLogs_WithValidPayload_ShouldPublishEventToKafka()
    {
        await using var factory = new IngestionApiFactory(
            _kafka,
            _redis,
            TestPublisherMode.RealKafka);

        var apiKey = "tf_test_kafka_publish_key";
        var workspaceId = Ulid.NewUlid();
        var projectId = Ulid.NewUlid();
        var applicationId = Ulid.NewUlid();
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-2);

        factory.ApiKeyValidator.Register(
            apiKey,
            new ApiKeyValidationResult(
                true,
                workspaceId,
                projectId,
                applicationId,
                "production"));

        using var consumer =
            new KafkaTestConsumer<EnrichedLogEvent>(
                _kafka.BootstrapServers,
                "traceflow.logs");

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", apiKey);

        var response = await client.PostAsJsonAsync("/logs", new
        {
            timestamp,
            level = LogLevel.Error,
            service = "checkout-api",
            message = "Payment failed",
            traceId = "trace-kafka-1",
            correlationId = "corr-kafka-1",
            metadata = new Dictionary<string, object?>
            {
                ["orderId"] = "order-456"
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<IngestLogResponse>();
        var kafkaEvent = consumer.ConsumeOne(TimeSpan.FromSeconds(10));

        kafkaEvent.EventId.Should().Be(body!.EventId);
        kafkaEvent.WorkspaceId.Should().Be(workspaceId);
        kafkaEvent.ProjectId.Should().Be(projectId);
        kafkaEvent.ApplicationId.Should().Be(applicationId);
        kafkaEvent.Environment.Should().Be("production");

        kafkaEvent.Timestamp.Should().Be(timestamp);
        kafkaEvent.Level.Should().Be(LogLevel.Error);
        kafkaEvent.Service.Should().Be("checkout-api");
        kafkaEvent.Message.Should().Be("Payment failed");
        kafkaEvent.TraceId.Should().Be("trace-kafka-1");
        kafkaEvent.CorrelationId.Should().Be("corr-kafka-1");
        kafkaEvent.Metadata.Should().ContainKey("orderId");
    }
}