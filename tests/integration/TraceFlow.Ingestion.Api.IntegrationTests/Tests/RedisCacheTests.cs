using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TraceFlow.Ingestion.Api.Contracts.Authentication;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Tests;

[Collection("Ingestion API Integration")]
public sealed class RedisCacheTests
{
    private readonly KafkaFixture _kafka;
    private readonly RedisFixture _redis;

    public RedisCacheTests(KafkaFixture kafka, RedisFixture redis)
    {
        _kafka = kafka;
        _redis = redis;
    }

    [Fact]
    public async Task PostLogs_WhenTenantContextCacheMiss_ShouldValidateApiKeyAndCacheTenantContext()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var apiKey = "tf_test_cache_miss_key";

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
            level = LogLevel.Information,
            service = "checkout-api",
            message = "Cache miss"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.ApiKeyValidator.CallCount.Should().Be(1);
        factory.LogEventPublisher.PublishedEvents.Should().ContainSingle();
    }

    [Fact]
    public async Task PostLogs_WhenTenantContextCacheHit_ShouldNotValidateApiKeyAgain()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var apiKey = "tf_test_cache_hit_key";
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

        var firstResponse = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Information,
            service = "checkout-api",
            message = "First request"
        });

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.ApiKeyValidator.CallCount.Should().Be(1);

        var secondResponse = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Information,
            service = "checkout-api",
            message = "Second request"
        });

        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.ApiKeyValidator.CallCount.Should().Be(1);

        factory.LogEventPublisher.PublishedEvents.Should().HaveCount(2);

        var secondEvent = factory.LogEventPublisher.PublishedEvents[1];

        secondEvent.WorkspaceId.Should().Be(workspaceId);
        secondEvent.ProjectId.Should().Be(projectId);
        secondEvent.ApplicationId.Should().Be(applicationId);
        secondEvent.Environment.Should().Be("staging");
    }

    [Fact]
    public async Task PostLogs_WithDifferentApiKeys_ShouldUseSeparateTenantContextCacheEntries()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var firstApiKey = "tf_test_cache_key_a";
        var firstWorkspaceId = Ulid.NewUlid();
        var firstProjectId = Ulid.NewUlid();
        var firstApplicationId = Ulid.NewUlid();

        var secondApiKey = "tf_test_cache_key_b";
        var secondWorkspaceId = Ulid.NewUlid();
        var secondProjectId = Ulid.NewUlid();
        var secondApplicationId = Ulid.NewUlid();

        factory.ApiKeyValidator.Register(
            firstApiKey,
            new ApiKeyValidationResult(
                true,
                firstWorkspaceId,
                firstProjectId,
                firstApplicationId,
                "production"));

        factory.ApiKeyValidator.Register(
            secondApiKey,
            new ApiKeyValidationResult(
                true,
                secondWorkspaceId,
                secondProjectId,
                secondApplicationId,
                "staging"));

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", firstApiKey);

        var firstResponse = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Information,
            service = "checkout-api",
            message = "First tenant"
        });

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", secondApiKey);

        var secondResponse = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Information,
            service = "checkout-api",
            message = "Second tenant"
        });

        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        factory.ApiKeyValidator.CallCount.Should().Be(2);
        factory.LogEventPublisher.PublishedEvents.Should().HaveCount(2);

        var firstEvent = factory.LogEventPublisher.PublishedEvents[0];
        var secondEvent = factory.LogEventPublisher.PublishedEvents[1];

        firstEvent.WorkspaceId.Should().Be(firstWorkspaceId);
        firstEvent.ProjectId.Should().Be(firstProjectId);
        firstEvent.ApplicationId.Should().Be(firstApplicationId);
        firstEvent.Environment.Should().Be("production");

        secondEvent.WorkspaceId.Should().Be(secondWorkspaceId);
        secondEvent.ProjectId.Should().Be(secondProjectId);
        secondEvent.ApplicationId.Should().Be(secondApplicationId);
        secondEvent.Environment.Should().Be("staging");
    }
}