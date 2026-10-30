using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Tests;

[Collection("Ingestion API Integration")]
public sealed class AuthenticationTests
{
    private readonly KafkaFixture _kafka;
    private readonly RedisFixture _redis;

    public AuthenticationTests(KafkaFixture kafka, RedisFixture redis)
    {
        _kafka = kafka;
        _redis = redis;
    }

    [Fact]
    public async Task PostLogs_WithInvalidApiKey_ShouldReturnUnauthorized()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "ApiKey",
                "tf_test_invalid_key");

        var response = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Error,
            service = "checkout-api",
            message = "Payment failed"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.ApiKeyValidator.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task PostLogs_WithRevokedApiKey_ShouldReturnUnauthorized()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var apiKey = "tf_test_revoked_key";

        factory.ApiKeyValidator.Register(
            apiKey,
            new TraceFlow.Ingestion.Api.Contracts.Authentication.ApiKeyValidationResult(
                false,
                null,
                null,
                null,
                null));

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "ApiKey",
                apiKey);

        var response = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Error,
            service = "checkout-api",
            message = "Payment failed"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.ApiKeyValidator.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task PostLogs_WithExpiredApiKey_ShouldReturnUnauthorized()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var apiKey = "tf_test_expired_key";

        factory.ApiKeyValidator.Register(
            apiKey,
            new TraceFlow.Ingestion.Api.Contracts.Authentication.ApiKeyValidationResult(
                false,
                null,
                null,
                null,
                null));

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "ApiKey",
                apiKey);

        var response = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Error,
            service = "checkout-api",
            message = "Payment failed"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.ApiKeyValidator.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task PostLogs_WithoutAuthorizationHeader_ShouldReturnUnauthorized()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Error,
            service = "checkout-api",
            message = "Payment failed"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.ApiKeyValidator.CallCount.Should().Be(0);
    }
    [Fact]
    public async Task PostLogs_WithWrongAuthorizationScheme_ShouldReturnUnauthorized()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);
        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "token");

        var response = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Error,
            service = "checkout-api",
            message = "Payment failed"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.ApiKeyValidator.CallCount.Should().Be(0);
    }
    [Fact]
    public async Task PostLogs_WithEmptyApiKeySecret_ShouldReturnUnauthorized()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);
        var client = factory.CreateClient();

        client.DefaultRequestHeaders.TryAddWithoutValidation(
            "Authorization",
            "ApiKey ");

        var response = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Error,
            service = "checkout-api",
            message = "Payment failed"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.ApiKeyValidator.CallCount.Should().Be(0);
    }
}
