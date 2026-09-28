using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TraceFlow.E2ETests.Infrastructure;
using TraceFlow.Ingestion.Api.Contracts.Authentication;
using TraceFlow.Ingestion.Api.Contracts.Log;

namespace TraceFlow.E2ETests.Tests;

[Collection("TraceFlow E2E")]
public sealed class TraceFlowPipelineTests
{
    private readonly KafkaFixture _kafka;
    private readonly RedisFixture _redis;
    private readonly OpenSearchFixture _openSearch;

    public TraceFlowPipelineTests(
        KafkaFixture kafka,
        RedisFixture redis,
        OpenSearchFixture openSearch)
    {
        _kafka = kafka;
        _redis = redis;
        _openSearch = openSearch;
    }

    [Fact]
    public async Task TraceFlowPipeline_WhenSingleLogIsIngested_ShouldIndexLogIntoOpenSearch()
    {
        await using var ingestionFactory =
            new IngestionApiFactory(_kafka, _redis);

        using var logProcessorHost = LogProcessorHostFactory.Create(
            _kafka,
            _openSearch,
            groupId: $"traceflow-e2e-log-processor-{Guid.NewGuid()}");

        var apiKey = "tf_e2e_single_log_key";
        var workspaceId = Ulid.NewUlid();
        var projectId = Ulid.NewUlid();
        var applicationId = Ulid.NewUlid();
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-1);

        ingestionFactory.ApiKeyValidator.Register(
            apiKey,
            new ApiKeyValidationResult(
                true,
                workspaceId,
                projectId,
                applicationId,
                "Production"));

        await logProcessorHost.StartAsync();

        try
        {
            var client = ingestionFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "ApiKey",
                    apiKey);

            var response = await client.PostAsJsonAsync("/logs", new
            {
                timestamp,
                level = LogLevel.Error,
                service = " checkout-api ",
                message = " Payment failed ",
                traceId = "trace-e2e-1",
                correlationId = "corr-e2e-1",
                metadata = new Dictionary<string, object?>
                {
                    ["orderId"] = "order-e2e-123"
                }
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content
                .ReadFromJsonAsync<IngestLogResponse>();

            body.Should().NotBeNull();
            body!.Status.Should().BeTrue();

            var indexedEvent = await _openSearch.WaitForLogEventAsync(
                body.EventId.ToString(),
                TimeSpan.FromSeconds(30));

            indexedEvent.EventId.Should().Be(body.EventId.ToString());
            indexedEvent.WorkspaceId.Should().Be(workspaceId.ToString());
            indexedEvent.ProjectId.Should().Be(projectId.ToString());
            indexedEvent.ApplicationId.Should().Be(applicationId.ToString());

            indexedEvent.Environment.Should().Be("production");
            indexedEvent.Service.Should().Be("checkout-api");
            indexedEvent.Message.Should().Be("Payment failed");

            indexedEvent.Level.Should().Be((int)LogLevel.Error);
            indexedEvent.TraceId.Should().Be("trace-e2e-1");
            indexedEvent.CorrelationId.Should().Be("corr-e2e-1");
        }
        finally
        {
            await logProcessorHost.StopAsync();
        }
    }
}