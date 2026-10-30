using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TraceFlow.Ingestion.Api.Contracts.Authentication;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Tests;

[Collection("Ingestion API Integration")]
public sealed class TenantIsolationTests
{
    private readonly KafkaFixture _kafka;
    private readonly RedisFixture _redis;

    public TenantIsolationTests(KafkaFixture kafka, RedisFixture redis)
    {
        _kafka = kafka;
        _redis = redis;
    }

    [Fact]
    public async Task PostLogs_WithDifferentApiKeys_ShouldPublishEventsUnderDifferentTenants()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var tenantAApiKey = "tf_test_tenant_a_key";
        var tenantAWorkspaceId = Ulid.NewUlid();
        var tenantAProjectId = Ulid.NewUlid();
        var tenantAApplicationId = Ulid.NewUlid();

        var tenantBApiKey = "tf_test_tenant_b_key";
        var tenantBWorkspaceId = Ulid.NewUlid();
        var tenantBProjectId = Ulid.NewUlid();
        var tenantBApplicationId = Ulid.NewUlid();

        factory.ApiKeyValidator.Register(
            tenantAApiKey,
            new ApiKeyValidationResult(
                true,
                tenantAWorkspaceId,
                tenantAProjectId,
                tenantAApplicationId,
                "production"));

        factory.ApiKeyValidator.Register(
            tenantBApiKey,
            new ApiKeyValidationResult(
                true,
                tenantBWorkspaceId,
                tenantBProjectId,
                tenantBApplicationId,
                "staging"));

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", tenantAApiKey);

        var tenantAResponse = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Information,
            service = "checkout-api",
            message = "Same payload"
        });

        tenantAResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", tenantBApiKey);

        var tenantBResponse = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Information,
            service = "checkout-api",
            message = "Same payload"
        });

        tenantBResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        factory.LogEventPublisher.PublishedEvents.Should().HaveCount(2);

        var tenantAEvent = factory.LogEventPublisher.PublishedEvents[0];
        var tenantBEvent = factory.LogEventPublisher.PublishedEvents[1];

        tenantAEvent.WorkspaceId.Should().Be(tenantAWorkspaceId);
        tenantAEvent.ProjectId.Should().Be(tenantAProjectId);
        tenantAEvent.ApplicationId.Should().Be(tenantAApplicationId);
        tenantAEvent.Environment.Should().Be("production");

        tenantBEvent.WorkspaceId.Should().Be(tenantBWorkspaceId);
        tenantBEvent.ProjectId.Should().Be(tenantBProjectId);
        tenantBEvent.ApplicationId.Should().Be(tenantBApplicationId);
        tenantBEvent.Environment.Should().Be("staging");

        tenantAEvent.WorkspaceId.Should().NotBe(tenantBEvent.WorkspaceId);
        tenantAEvent.ProjectId.Should().NotBe(tenantBEvent.ProjectId);
        tenantAEvent.ApplicationId.Should().NotBe(tenantBEvent.ApplicationId);
    }

    [Fact]
    public async Task PostLogs_WithTenantBMetadataButTenantAApiKey_ShouldPublishUnderTenantA()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var tenantAApiKey = "tf_test_tenant_a_override_key";
        var tenantAWorkspaceId = Ulid.NewUlid();
        var tenantAProjectId = Ulid.NewUlid();
        var tenantAApplicationId = Ulid.NewUlid();

        var tenantBWorkspaceId = Ulid.NewUlid();
        var tenantBProjectId = Ulid.NewUlid();
        var tenantBApplicationId = Ulid.NewUlid();

        factory.ApiKeyValidator.Register(
            tenantAApiKey,
            new ApiKeyValidationResult(
                true,
                tenantAWorkspaceId,
                tenantAProjectId,
                tenantAApplicationId,
                "production"));

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", tenantAApiKey);

        var response = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Warning,
            service = "checkout-api",
            message = "Tenant isolation attempt",
            metadata = new Dictionary<string, object?>
            {
                ["workspaceId"] = tenantBWorkspaceId.ToString(),
                ["projectId"] = tenantBProjectId.ToString(),
                ["applicationId"] = tenantBApplicationId.ToString(),
                ["environment"] = "staging"
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        factory.LogEventPublisher.PublishedEvents.Should().ContainSingle();

        var publishedEvent = factory.LogEventPublisher.PublishedEvents.Single();

        publishedEvent.WorkspaceId.Should().Be(tenantAWorkspaceId);
        publishedEvent.ProjectId.Should().Be(tenantAProjectId);
        publishedEvent.ApplicationId.Should().Be(tenantAApplicationId);
        publishedEvent.Environment.Should().Be("production");

        publishedEvent.WorkspaceId.Should().NotBe(tenantBWorkspaceId);
        publishedEvent.ProjectId.Should().NotBe(tenantBProjectId);
        publishedEvent.ApplicationId.Should().NotBe(tenantBApplicationId);
    }
}