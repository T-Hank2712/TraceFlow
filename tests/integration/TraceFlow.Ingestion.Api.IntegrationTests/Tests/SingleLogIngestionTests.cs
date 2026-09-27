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
    public async Task PostLogs_WithValidApiKey_ShouldAcceptRequest()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var apiKey = "tf_test_valid_key";

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
        factory.ApiKeyValidator.CallCount.Should().Be(1);
    }
}