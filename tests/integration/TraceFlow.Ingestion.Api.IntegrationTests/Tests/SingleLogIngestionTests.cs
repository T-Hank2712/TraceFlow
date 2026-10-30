using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TraceFlow.Ingestion.Api.Contracts.Authentication;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Tests;

[Collection("Ingestion API Integration")]
public sealed class SingleLogIngestionTests
{
    private readonly KafkaFixture _kafka;
    private readonly RedisFixture _redis;

    public SingleLogIngestionTests(KafkaFixture kafka, RedisFixture redis)
    {
        _kafka = kafka;
        _redis = redis;
    }

    [Fact]
    public async Task PostLogs_WithValidPayload_ShouldReturnSuccessResponse()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var apiKey = "tf_test_single_log_key";

        factory.ApiKeyValidator.Register(
            apiKey,
            new ApiKeyValidationResult(
                true,
                Ulid.NewUlid(),
                Ulid.NewUlid(),
                Ulid.NewUlid(),
                "production"));

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", apiKey);

        var response = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Error,
            service = "checkout-api",
            message = "Payment failed",
            traceId = "trace-1",
            correlationId = "corr-1",
            metadata = new Dictionary<string, object?>
            {
                ["orderId"] = "order-123"
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<IngestLogResponse>();

        body.Should().NotBeNull();
        body!.Status.Should().BeTrue();
        body.EventId.Should().NotBe(default(Ulid));
        body.AcceptedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task PostLogs_WithValidPayload_ShouldPassEnrichedEventToPublisher()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var apiKey = "tf_test_single_log_publish_key";
        var workspaceId = Ulid.NewUlid();
        var projectId = Ulid.NewUlid();
        var applicationId = Ulid.NewUlid();
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-5);

        factory.ApiKeyValidator.Register(
            apiKey,
            new ApiKeyValidationResult(
                true,
                workspaceId,
                projectId,
                applicationId,
                "production"));

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", apiKey);

        var response = await client.PostAsJsonAsync("/logs", new
        {
            timestamp,
            level = LogLevel.Error,
            service = " checkout-api ",
            message = " Payment failed ",
            traceId = "trace-1",
            correlationId = "corr-1",
            metadata = new Dictionary<string, object?>
            {
                ["orderId"] = "order-123"
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<IngestLogResponse>();

        factory.LogEventPublisher.PublishedEvents.Should().ContainSingle();

        var publishedEvent = factory.LogEventPublisher.PublishedEvents.Single();

        publishedEvent.WorkspaceId.Should().Be(workspaceId);
        publishedEvent.ProjectId.Should().Be(projectId);
        publishedEvent.ApplicationId.Should().Be(applicationId);
        publishedEvent.Environment.Should().Be("production");

        publishedEvent.Timestamp.Should().Be(timestamp);
        publishedEvent.Level.Should().Be(LogLevel.Error);
        publishedEvent.Service.Should().Be("checkout-api");
        publishedEvent.Message.Should().Be("Payment failed");
        publishedEvent.TraceId.Should().Be("trace-1");
        publishedEvent.CorrelationId.Should().Be("corr-1");
        publishedEvent.Metadata.Should().ContainKey("orderId");

        publishedEvent.ReceivedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));

        body!.EventId.Should().Be(publishedEvent.EventId);
    }
}