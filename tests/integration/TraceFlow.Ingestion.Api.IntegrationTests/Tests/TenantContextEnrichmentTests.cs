using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TraceFlow.Ingestion.Api.Contracts.Authentication;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

namespace TraceFlow.Ingestion.Api.IntegrationTests.Tests;

[Collection("Ingestion API Integration")]
public sealed class TenantContextEnrichmentTests
{
    private readonly KafkaFixture _kafka;
    private readonly RedisFixture _redis;

    public TenantContextEnrichmentTests(KafkaFixture kafka, RedisFixture redis)
    {
        _kafka = kafka;
        _redis = redis;
    }

    [Fact]
    public async Task PostLogs_WithValidApiKey_ShouldEnrichEventWithTenantContextFromApiKey()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var apiKey = "tf_test_tenant_context_key";
        var workspaceId = Ulid.NewUlid();
        var projectId = Ulid.NewUlid();
        var applicationId = Ulid.NewUlid();
        var environment = "production";

        factory.ApiKeyValidator.Register(
            apiKey,
            new ApiKeyValidationResult(
                true,
                workspaceId,
                projectId,
                applicationId,
                environment));

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", apiKey);

        var response = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Information,
            service = "checkout-api",
            message = "Checkout started"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        factory.LogEventPublisher.PublishedEvents.Should().ContainSingle();

        var publishedEvent = factory.LogEventPublisher.PublishedEvents.Single();

        publishedEvent.WorkspaceId.Should().Be(workspaceId);
        publishedEvent.ProjectId.Should().Be(projectId);
        publishedEvent.ApplicationId.Should().Be(applicationId);
        publishedEvent.Environment.Should().Be(environment);
    }
    [Fact]
    public async Task PostLogs_WithTenantLikeMetadata_ShouldStillUseTenantContextFromApiKey()
    {
        await using var factory = new IngestionApiFactory(_kafka, _redis);

        var apiKey = "tf_test_tenant_context_override_key";
        var workspaceId = Ulid.NewUlid();
        var projectId = Ulid.NewUlid();
        var applicationId = Ulid.NewUlid();

        var fakeWorkspaceId = Ulid.NewUlid();
        var fakeProjectId = Ulid.NewUlid();
        var fakeApplicationId = Ulid.NewUlid();

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

        var response = await client.PostAsJsonAsync("/logs", new
        {
            timestamp = DateTimeOffset.UtcNow,
            level = LogLevel.Warning,
            service = "checkout-api",
            message = "Tenant override attempt",
            metadata = new Dictionary<string, object?>
            {
                ["workspaceId"] = fakeWorkspaceId.ToString(),
                ["projectId"] = fakeProjectId.ToString(),
                ["applicationId"] = fakeApplicationId.ToString(),
                ["environment"] = "fake-production"
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var publishedEvent = factory.LogEventPublisher.PublishedEvents.Single();

        publishedEvent.WorkspaceId.Should().Be(workspaceId);
        publishedEvent.ProjectId.Should().Be(projectId);
        publishedEvent.ApplicationId.Should().Be(applicationId);
        publishedEvent.Environment.Should().Be("staging");

        publishedEvent.WorkspaceId.Should().NotBe(fakeWorkspaceId);
        publishedEvent.ProjectId.Should().NotBe(fakeProjectId);
        publishedEvent.ApplicationId.Should().NotBe(fakeApplicationId);
    }
}