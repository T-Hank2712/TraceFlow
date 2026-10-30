using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TraceFlow.Ingestion.Api.Contracts.Authentication;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Tests;

[Collection("Ingestion API Integration")]
public sealed class ValidationTests
{
    private readonly KafkaFixture _kafka;
    private readonly RedisFixture _redis;

    public ValidationTests(KafkaFixture kafka, RedisFixture redis)
    {
        _kafka = kafka;
        _redis = redis;
    }

    [Theory]
    [MemberData(nameof(InvalidSingleLogPayloads))]
    public async Task PostLogs_WithInvalidPayload_ShouldReturnBadRequest(object payload)
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var apiKey = "tf_test_validation_key";

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

        var response = await client.PostAsJsonAsync("/logs", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        factory.LogEventPublisher.PublishedEvents.Should().BeEmpty();
    }

    public static IEnumerable<object[]> InvalidSingleLogPayloads()
    {
        yield return
        [
            new
            {
                timestamp = DateTimeOffset.UtcNow,
                level = LogLevel.Error,
                service = "",
                message = "Payment failed"
            }
        ];

        yield return
        [
            new
            {
                timestamp = DateTimeOffset.UtcNow,
                level = LogLevel.Error,
                service = "checkout-api",
                message = ""
            }
        ];

        yield return
        [
            new
            {
                timestamp = DateTimeOffset.UtcNow,
                level = LogLevel.None,
                service = "checkout-api",
                message = "Payment failed"
            }
        ];

        yield return
        [
            new
            {
                timestamp = DateTimeOffset.UtcNow,
                level = LogLevel.Error,
                service = new string('s', 65),
                message = "Payment failed"
            }
        ];

        yield return
        [
            new
            {
                timestamp = DateTimeOffset.UtcNow,
                level = LogLevel.Error,
                service = "checkout-api",
                message = new string('m', 129)
            }
        ];
    }

    [Fact]
    public async Task PostBatchLogs_WithEmptyLogs_ShouldReturnBadRequest()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var apiKey = "tf_test_empty_batch_key";

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
            logs = Array.Empty<object>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        factory.LogEventPublisher.PublishedEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task PostBatchLogs_WithMoreThanConfiguredMaxBatchSize_ShouldReturnBadRequest()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var apiKey = "tf_test_oversized_batch_key";

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
                new { level = LogLevel.Information, service = "api-1", message = "one" },
                new { level = LogLevel.Information, service = "api-2", message = "two" },
                new { level = LogLevel.Information, service = "api-3", message = "three" },
                new { level = LogLevel.Information, service = "api-4", message = "four" }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        factory.LogEventPublisher.PublishedEvents.Should().BeEmpty();
    }
}