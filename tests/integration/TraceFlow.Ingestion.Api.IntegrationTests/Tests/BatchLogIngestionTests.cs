using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TraceFlow.Ingestion.Api.Contracts.Authentication;
using TraceFlow.Ingestion.Api.Contracts.BatchLog;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Tests;

[Collection("Ingestion API Integration")]
public sealed class BatchLogIngestionTests
{
    private readonly KafkaFixture _kafka;
    private readonly RedisFixture _redis;

    public BatchLogIngestionTests(KafkaFixture kafka, RedisFixture redis)
    {
        _kafka = kafka;
        _redis = redis;
    }

    [Fact]
    public async Task PostBatchLogs_WithValidPayload_ShouldReturnSuccessResponse()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var apiKey = "tf_test_batch_log_key";

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

        var response = await client.PostAsJsonAsync("/batch-logs", new
        {
            logs = new object[]
            {
                new
                {
                    timestamp = DateTimeOffset.UtcNow.AddMinutes(-2),
                    level = LogLevel.Information,
                    service = "checkout-api",
                    message = "Checkout started",
                    traceId = "trace-batch-1",
                    correlationId = "corr-batch-1",
                    metadata = new Dictionary<string, object?>
                    {
                        ["step"] = "start"
                    }
                },
                new
                {
                    timestamp = DateTimeOffset.UtcNow.AddMinutes(-1),
                    level = LogLevel.Error,
                    service = "payment-api",
                    message = "Payment failed",
                    traceId = "trace-batch-2",
                    correlationId = "corr-batch-2",
                    metadata = new Dictionary<string, object?>
                    {
                        ["orderId"] = "order-789"
                    }
                }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<BatchLogResponse>();

        body.Should().NotBeNull();
        body!.BatchId.Should().NotBe(default(Ulid));
        body.Total.Should().Be(2);
        body.Accepted.Should().Be(2);
        body.Rejected.Should().Be(0);
        body.Results.Should().HaveCount(2);
        body.Results.Should().OnlyContain(result => result.Accepted);
        body.Results.Should().OnlyContain(result => result.EventId != null);
        body.Results.Should().OnlyContain(result => result.Error == null);
    }

    [Fact]
    public async Task PostBatchLogs_WithValidPayload_ShouldPassAllEventsToPublisher()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var apiKey = "tf_test_batch_log_publish_key";
        var workspaceId = Ulid.NewUlid();
        var projectId = Ulid.NewUlid();
        var applicationId = Ulid.NewUlid();

        factory.ApiKeyValidator.Register(
            apiKey,
            new ApiKeyValidationResult(
                true,
                workspaceId,
                projectId,
                applicationId,
                "staging"));

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", apiKey);

        var response = await client.PostAsJsonAsync("/batch-logs", new
        {
            logs = new object[]
            {
                new
                {
                    timestamp = DateTimeOffset.UtcNow.AddMinutes(-2),
                    level = LogLevel.Information,
                    service = " checkout-api ",
                    message = " Checkout started ",
                    traceId = "trace-batch-1",
                    correlationId = "corr-batch-1",
                    metadata = new Dictionary<string, object?>
                    {
                        ["step"] = "start"
                    }
                },
                new
                {
                    timestamp = DateTimeOffset.UtcNow.AddMinutes(-1),
                    level = LogLevel.Warning,
                    service = " payment-api ",
                    message = " Slow payment response ",
                    traceId = "trace-batch-2",
                    correlationId = "corr-batch-2",
                    metadata = new Dictionary<string, object?>
                    {
                        ["orderId"] = "order-789"
                    }
                }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<BatchLogResponse>();

        factory.LogEventPublisher.PublishedEvents.Should().HaveCount(2);

        var firstEvent = factory.LogEventPublisher.PublishedEvents[0];
        var secondEvent = factory.LogEventPublisher.PublishedEvents[1];

        firstEvent.BatchID.Should().Be(body!.BatchId);
        secondEvent.BatchID.Should().Be(body.BatchId);

        firstEvent.WorkspaceId.Should().Be(workspaceId);
        firstEvent.ProjectId.Should().Be(projectId);
        firstEvent.ApplicationId.Should().Be(applicationId);
        firstEvent.Environment.Should().Be("staging");

        secondEvent.WorkspaceId.Should().Be(workspaceId);
        secondEvent.ProjectId.Should().Be(projectId);
        secondEvent.ApplicationId.Should().Be(applicationId);
        secondEvent.Environment.Should().Be("staging");

        firstEvent.Service.Should().Be("checkout-api");
        firstEvent.Message.Should().Be("Checkout started");
        firstEvent.TraceId.Should().Be("trace-batch-1");
        firstEvent.CorrelationId.Should().Be("corr-batch-1");

        secondEvent.Service.Should().Be("payment-api");
        secondEvent.Message.Should().Be("Slow payment response");
        secondEvent.TraceId.Should().Be("trace-batch-2");
        secondEvent.CorrelationId.Should().Be("corr-batch-2");

        body.Results[0].EventId.Should().Be(firstEvent.EventId);
        body.Results[1].EventId.Should().Be(secondEvent.EventId);
    }
}