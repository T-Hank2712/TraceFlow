using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TraceFlow.E2ETests.Infrastructure;
using TraceFlow.Ingestion.Api.Contracts.Authentication;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.Contracts.BatchLog;

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
    [Fact]
    public async Task TraceFlowPipeline_WhenBatchLogsAreIngested_ShouldIndexAllLogsIntoOpenSearch()
    {
        await using var ingestionFactory =
            new IngestionApiFactory(_kafka, _redis);

        using var logProcessorHost = LogProcessorHostFactory.Create(
            _kafka,
            _openSearch,
            groupId: $"traceflow-e2e-log-processor-batch-{Guid.NewGuid()}");

        var apiKey = "tf_e2e_batch_log_key";
        var workspaceId = Ulid.NewUlid();
        var projectId = Ulid.NewUlid();
        var applicationId = Ulid.NewUlid();

        var firstTimestamp = DateTimeOffset.UtcNow.AddMinutes(-2);
        var secondTimestamp = DateTimeOffset.UtcNow.AddMinutes(-1);

        ingestionFactory.ApiKeyValidator.Register(
            apiKey,
            new ApiKeyValidationResult(
                true,
                workspaceId,
                projectId,
                applicationId,
                "Staging"));

        await logProcessorHost.StartAsync();

        try
        {
            var client = ingestionFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "ApiKey",
                    apiKey);

            var response = await client.PostAsJsonAsync("/batch-logs", new
            {
                logs = new object[]
                {
                    new
                    {
                        timestamp = firstTimestamp,
                        level = LogLevel.Information,
                        service = " checkout-api ",
                        message = " Checkout started ",
                        traceId = "trace-e2e-batch-1",
                        correlationId = "corr-e2e-batch-1",
                        metadata = new Dictionary<string, object?>
                        {
                            ["step"] = "start"
                        }
                    },
                    new
                    {
                        timestamp = secondTimestamp,
                        level = LogLevel.Error,
                        service = " payment-api ",
                        message = " Payment failed ",
                        traceId = "trace-e2e-batch-2",
                        correlationId = "corr-e2e-batch-2",
                        metadata = new Dictionary<string, object?>
                        {
                            ["orderId"] = "order-e2e-batch-456"
                        }
                    }
                }
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<BatchLogResponse>();

            body.Should().NotBeNull();
            body!.Total.Should().Be(2);
            body.Accepted.Should().Be(2);
            body.Rejected.Should().Be(0);
            body.Results.Should().HaveCount(2);
            body.Results.Should().OnlyContain(result => result.Accepted);
            body.Results.Should().OnlyContain(result => result.EventId.HasValue);

            var firstEventId = body.Results[0].EventId!.Value.ToString();
            var secondEventId = body.Results[1].EventId!.Value.ToString();

            var firstIndexedEvent = await _openSearch.WaitForLogEventAsync(
                firstEventId,
                TimeSpan.FromSeconds(30));

            var secondIndexedEvent = await _openSearch.WaitForLogEventAsync(
                secondEventId,
                TimeSpan.FromSeconds(30));

            firstIndexedEvent.EventId.Should().Be(firstEventId);
            firstIndexedEvent.WorkspaceId.Should().Be(workspaceId.ToString());
            firstIndexedEvent.ProjectId.Should().Be(projectId.ToString());
            firstIndexedEvent.ApplicationId.Should().Be(applicationId.ToString());
            firstIndexedEvent.Environment.Should().Be("staging");
            firstIndexedEvent.Service.Should().Be("checkout-api");
            firstIndexedEvent.Message.Should().Be("Checkout started");
            firstIndexedEvent.Level.Should().Be((int)LogLevel.Information);
            firstIndexedEvent.TraceId.Should().Be("trace-e2e-batch-1");
            firstIndexedEvent.CorrelationId.Should().Be("corr-e2e-batch-1");

            secondIndexedEvent.EventId.Should().Be(secondEventId);
            secondIndexedEvent.WorkspaceId.Should().Be(workspaceId.ToString());
            secondIndexedEvent.ProjectId.Should().Be(projectId.ToString());
            secondIndexedEvent.ApplicationId.Should().Be(applicationId.ToString());
            secondIndexedEvent.Environment.Should().Be("staging");
            secondIndexedEvent.Service.Should().Be("payment-api");
            secondIndexedEvent.Message.Should().Be("Payment failed");
            secondIndexedEvent.Level.Should().Be((int)LogLevel.Error);
            secondIndexedEvent.TraceId.Should().Be("trace-e2e-batch-2");
            secondIndexedEvent.CorrelationId.Should().Be("corr-e2e-batch-2");
        }
        finally
        {
            await logProcessorHost.StopAsync();
        }
    }
    [Fact]
    public async Task TraceFlowPipeline_WhenLogIsIngested_ShouldPersistApiKeyTenantContextInOpenSearch()
    {
        await using var ingestionFactory =
            new IngestionApiFactory(_kafka, _redis);

        using var logProcessorHost = LogProcessorHostFactory.Create(
            _kafka,
            _openSearch,
            groupId: $"traceflow-e2e-log-processor-enrichment-{Guid.NewGuid()}");

        var apiKey = "tf_e2e_enrichment_key";
        var workspaceId = Ulid.NewUlid();
        var projectId = Ulid.NewUlid();
        var applicationId = Ulid.NewUlid();

        var fakeWorkspaceId = Ulid.NewUlid();
        var fakeProjectId = Ulid.NewUlid();
        var fakeApplicationId = Ulid.NewUlid();

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
                timestamp = DateTimeOffset.UtcNow,
                level = LogLevel.Warning,
                service = "inventory-api",
                message = "Tenant enrichment check",
                traceId = "trace-e2e-enrichment-1",
                correlationId = "corr-e2e-enrichment-1",
                metadata = new Dictionary<string, object?>
                {
                    ["workspaceId"] = fakeWorkspaceId.ToString(),
                    ["projectId"] = fakeProjectId.ToString(),
                    ["applicationId"] = fakeApplicationId.ToString(),
                    ["environment"] = "fake-production"
                }
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content
                .ReadFromJsonAsync<IngestLogResponse>();

            body.Should().NotBeNull();

            var indexedEvent = await _openSearch.WaitForLogEventAsync(
                body!.EventId.ToString(),
                TimeSpan.FromSeconds(30));

            indexedEvent.WorkspaceId.Should().Be(workspaceId.ToString());
            indexedEvent.ProjectId.Should().Be(projectId.ToString());
            indexedEvent.ApplicationId.Should().Be(applicationId.ToString());
            indexedEvent.Environment.Should().Be("production");

            indexedEvent.WorkspaceId.Should().NotBe(fakeWorkspaceId.ToString());
            indexedEvent.ProjectId.Should().NotBe(fakeProjectId.ToString());
            indexedEvent.ApplicationId.Should().NotBe(fakeApplicationId.ToString());

            indexedEvent.Service.Should().Be("inventory-api");
            indexedEvent.Message.Should().Be("Tenant enrichment check");
            indexedEvent.TraceId.Should().Be("trace-e2e-enrichment-1");
            indexedEvent.CorrelationId.Should().Be("corr-e2e-enrichment-1");
        }
        finally
        {
            await logProcessorHost.StopAsync();
        }
    }
    [Fact]
    public async Task TraceFlowPipeline_WhenLogIsProcessed_ShouldIndexCompleteDocumentInOpenSearch()
    {
        await using var ingestionFactory =
            new IngestionApiFactory(_kafka, _redis);

        using var logProcessorHost = LogProcessorHostFactory.Create(
            _kafka,
            _openSearch,
            groupId: $"traceflow-e2e-log-processor-index-shape-{Guid.NewGuid()}");

        var apiKey = "tf_e2e_index_shape_key";
        var workspaceId = Ulid.NewUlid();
        var projectId = Ulid.NewUlid();
        var applicationId = Ulid.NewUlid();
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-3);

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
                service = " billing-api ",
                message = " Invoice generation failed ",
                traceId = "trace-e2e-index-1",
                correlationId = "corr-e2e-index-1",
                metadata = new Dictionary<string, object?>
                {
                    ["invoiceId"] = "invoice-123",
                    ["attempt"] = 3,
                    ["retryable"] = true
                }
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content
                .ReadFromJsonAsync<IngestLogResponse>();

            body.Should().NotBeNull();

            var indexedEvent = await _openSearch.WaitForLogEventAsync(
                body!.EventId.ToString(),
                TimeSpan.FromSeconds(30));

            indexedEvent.EventId.Should().Be(body.EventId.ToString());

            indexedEvent.WorkspaceId.Should().Be(workspaceId.ToString());
            indexedEvent.ProjectId.Should().Be(projectId.ToString());
            indexedEvent.ApplicationId.Should().Be(applicationId.ToString());
            indexedEvent.Environment.Should().Be("production");

            indexedEvent.Timestamp.Should().Be(timestamp.ToUniversalTime());
            indexedEvent.Level.Should().Be((int)LogLevel.Error);
            indexedEvent.Service.Should().Be("billing-api");
            indexedEvent.Message.Should().Be("Invoice generation failed");
            indexedEvent.TraceId.Should().Be("trace-e2e-index-1");
            indexedEvent.CorrelationId.Should().Be("corr-e2e-index-1");

            indexedEvent.Metadata.Should().NotBeNull();
            indexedEvent.Metadata!.Should().ContainKey("invoiceId");
            indexedEvent.Metadata.Should().ContainKey("attempt");
            indexedEvent.Metadata.Should().ContainKey("retryable");
        }
        finally
        {
            await logProcessorHost.StopAsync();
        }
    }
}