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
public sealed class BatchKafkaPublishingTests
{
    private readonly KafkaFixture _kafka;
    private readonly RedisFixture _redis;

    public BatchKafkaPublishingTests(KafkaFixture kafka, RedisFixture redis)
    {
        _kafka = kafka;
        _redis = redis;
    }

    [Fact]
    public async Task PostBatchLogs_WithMixedValidAndInvalidItems_ShouldPublishOnlyValidItemsToKafka()
    {
        await using var factory = new IngestionApiFactory(
            _kafka,
            _redis,
            useRealKafkaPublisher: true);

        var apiKey = "tf_test_batch_kafka_partial_key";
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
                "production"));

        using var consumer =
            new KafkaTestConsumer<EnrichedLogEvent>(
                _kafka.BootstrapServers,
                "traceflow.logs");

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
                    level = LogLevel.Warning,
                    service = "checkout-api",
                    message = "Slow checkout",
                    traceId = "trace-valid-batch-kafka",
                    correlationId = "corr-valid-batch-kafka",
                    metadata = new Dictionary<string, object?>
                    {
                        ["step"] = "valid"
                    }
                },
                new
                {
                    timestamp = DateTimeOffset.UtcNow.AddMinutes(-1),
                    level = LogLevel.Error,
                    service = "",
                    message = "Missing service",
                    traceId = "trace-invalid-batch-kafka",
                    correlationId = "corr-invalid-batch-kafka",
                    metadata = new Dictionary<string, object?>
                    {
                        ["step"] = "invalid"
                    }
                }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<BatchLogResponse>();

        body.Should().NotBeNull();
        body!.Total.Should().Be(2);
        body.Accepted.Should().Be(1);
        body.Rejected.Should().Be(1);

        body.Results[0].Accepted.Should().BeTrue();
        body.Results[0].EventId.Should().NotBeNull();

        body.Results[1].Accepted.Should().BeFalse();
        body.Results[1].EventId.Should().BeNull();

        var expectedEventId = body.Results[0].EventId!.Value;

        var kafkaEvent = consumer.ConsumeUntil(
            value => value.EventId == expectedEventId,
            TimeSpan.FromSeconds(10));

        kafkaEvent.EventId.Should().Be(expectedEventId);
        kafkaEvent.BatchID.Should().Be(body.BatchId);

        kafkaEvent.WorkspaceId.Should().Be(workspaceId);
        kafkaEvent.ProjectId.Should().Be(projectId);
        kafkaEvent.ApplicationId.Should().Be(applicationId);
        kafkaEvent.Environment.Should().Be("production");

        kafkaEvent.Service.Should().Be("checkout-api");
        kafkaEvent.Message.Should().Be("Slow checkout");
        kafkaEvent.TraceId.Should().Be("trace-valid-batch-kafka");
        kafkaEvent.CorrelationId.Should().Be("corr-valid-batch-kafka");
    }
}