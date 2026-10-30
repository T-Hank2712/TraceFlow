using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TraceFlow.Ingestion.Api.Contracts;
using TraceFlow.Ingestion.Api.Contracts.Authentication;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.Errors;
using TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Tests;

[Collection("Ingestion API Integration")]
public sealed class KafkaFailureTests
{
    private readonly KafkaFixture _kafka;
    private readonly RedisFixture _redis;

    public KafkaFailureTests(KafkaFixture kafka, RedisFixture redis)
    {
        _kafka = kafka;
        _redis = redis;
    }

    [Fact]
    public async Task PostLogs_WhenKafkaPublishFails_ShouldReturnServiceUnavailable()
    {
        await using var factory = new IngestionApiFactory(
            _kafka,
            _redis,
            TestPublisherMode.Failing);

        var apiKey = "tf_test_kafka_failure_key";

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
            message = "Payment failed"
        });

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        error.Should().NotBeNull();
        error!.Code.Should().Be(ErrorCodes.KafkaPublishFailed);
        error.Message.Should().Be("Failed to publish log event to Kafka.");
    }
}