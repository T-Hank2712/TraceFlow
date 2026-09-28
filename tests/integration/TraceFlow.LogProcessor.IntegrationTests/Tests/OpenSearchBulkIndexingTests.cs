using FluentAssertions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.IntegrationTests.Infrastructure;
using TraceFlow.LogProcessor.Services.Kafka;
using TraceFlow.LogProcessor.Services.OpenSearch;
using Microsoft.Extensions.DependencyInjection;

namespace TraceFlow.LogProcessor.IntegrationTests.Tests;

[Collection("Log Processor Integration")]
public sealed class OpenSearchBulkIndexingTests
{
    private readonly KafkaFixture _kafka;
    private readonly OpenSearchFixture _openSearch;

    public OpenSearchBulkIndexingTests(
        KafkaFixture kafka,
        OpenSearchFixture openSearch)
    {
        _kafka = kafka;
        _openSearch = openSearch;
    }

    [Fact]
    public async Task LogProcessor_WhenBulkIndexPartiallyFails_ShouldPublishFailedEventsToDlq()
    {
        var firstEventId = Ulid.NewUlid().ToString();
        var secondEventId = Ulid.NewUlid().ToString();

        var fakeIndexer = new PartialFailingLogIndexer();
        var dlqProducer = new RecordingDlqProducer();

        using var host = LogProcessorHostFactory.Create(
            _kafka,
            _openSearch,
            groupId: $"traceflow-log-processor-bulk-failure-test-{Guid.NewGuid()}",
            batchSize: 2,
            flushIntervalMs: 10_000,
            configureServices: services =>
            {
                services.RemoveAll<ILogIndexer>();
                services.AddSingleton<ILogIndexer>(fakeIndexer);

                services.RemoveAll<IDlqProducer>();
                services.AddSingleton<IDlqProducer>(dlqProducer);
            });

        await host.StartAsync();

        try
        {
            using var producer = new KafkaTestProducer(_kafka.BootstrapServers);

            var firstEvent = new LogEvent(
                EventId: firstEventId,
                WorkspaceId: Ulid.NewUlid().ToString(),
                ProjectId: Ulid.NewUlid().ToString(),
                ApplicationId: Ulid.NewUlid().ToString(),
                Environment: "production",
                Service: "checkout-api",
                Level: 2,
                Message: "Indexed event",
                Timestamp: DateTimeOffset.UtcNow,
                TraceId: "trace-bulk-1",
                CorrelationId: "corr-bulk-1",
                Metadata: null);

            var secondEvent = new LogEvent(
                EventId: secondEventId,
                WorkspaceId: Ulid.NewUlid().ToString(),
                ProjectId: Ulid.NewUlid().ToString(),
                ApplicationId: Ulid.NewUlid().ToString(),
                Environment: "production",
                Service: "payment-api",
                Level: 4,
                Message: "Failed event",
                Timestamp: DateTimeOffset.UtcNow,
                TraceId: "trace-bulk-2",
                CorrelationId: "corr-bulk-2",
                Metadata: null);

            await producer.ProduceLogEventAsync("traceflow.logs", firstEvent);
            await producer.ProduceLogEventAsync("traceflow.logs", secondEvent);

            await WaitUntilAsync(
                () => dlqProducer.PublishedEvents.Count == 1,
                TimeSpan.FromSeconds(20));

            fakeIndexer.ReceivedEvents.Should().HaveCount(2);

            dlqProducer.PublishedEvents.Should().ContainSingle();

            var dlqEvent = dlqProducer.PublishedEvents.Single();

            dlqEvent.Event.EventId.Should().Be(secondEventId);
            dlqEvent.FailureType.Should().Be("OpenSearchItemFailure");
            dlqEvent.FailureReason.Should().Be("OpenSearch failed to index the event.");
            dlqEvent.FailedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(20));
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private static async Task WaitUntilAsync(
        Func<bool> condition,
        TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);

        while (DateTimeOffset.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(250);
        }

        throw new TimeoutException("Condition was not met within the timeout.");
    }
}