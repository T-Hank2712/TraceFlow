using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.IntegrationTests.Infrastructure;
using TraceFlow.LogProcessor.Services.OpenSearch;

namespace TraceFlow.LogProcessor.IntegrationTests.Tests;

[Collection("Log Processor Integration")]
public sealed class KafkaOffsetManagementTests
{
    private readonly KafkaFixture _kafka;
    private readonly OpenSearchFixture _openSearch;

    public KafkaOffsetManagementTests(
        KafkaFixture kafka,
        OpenSearchFixture openSearch)
    {
        _kafka = kafka;
        _openSearch = openSearch;
    }

    [Fact]
    public async Task LogProcessor_AfterSuccessfulProcessing_ShouldCommitOffsetAndNotReprocessMessage()
    {
        var topic = $"traceflow.logs.offset-test-{Guid.NewGuid()}";
        var dlqTopic = $"{topic}.dlq";
        var groupId = $"traceflow-log-processor-offset-test-{Guid.NewGuid()}";
        var eventId = Ulid.NewUlid().ToString();

        await _kafka.CreateTopicAsync(topic);
        await _kafka.CreateTopicAsync(dlqTopic);

        using var producer = new KafkaTestProducer(_kafka.BootstrapServers);

        var logEvent = new LogEvent(
            EventId: eventId,
            WorkspaceId: Ulid.NewUlid().ToString(),
            ProjectId: Ulid.NewUlid().ToString(),
            ApplicationId: Ulid.NewUlid().ToString(),
            Environment: "production",
            Service: "checkout-api",
            Level: 2,
            Message: "Offset event",
            Timestamp: DateTimeOffset.UtcNow,
            TraceId: "trace-offset-1",
            CorrelationId: "corr-offset-1",
            Metadata: null);

        await producer.ProduceLogEventAsync(topic, logEvent);

        var firstIndexer = new CountingLogIndexer();

        using (var firstHost = LogProcessorHostFactory.Create(
            _kafka,
            _openSearch,
            groupId: groupId,
            batchSize: 1,
            flushIntervalMs: 200,
            topic: topic,
            dlqTopic: dlqTopic,
            configureServices: services =>
            {
                services.RemoveAll<ILogIndexer>();
                services.AddSingleton<ILogIndexer>(firstIndexer);
            }))
        {
            await firstHost.StartAsync();

            await WaitUntilAsync(
                () => firstIndexer.IndexedEvents.Any(x => x.EventId == eventId),
                TimeSpan.FromSeconds(20));

            // Give KafkaConsumer loop a moment to see the commit signal and commit offsets.
            await Task.Delay(1_000);

            await firstHost.StopAsync();
        }

        firstIndexer.IndexedEvents
            .Count(x => x.EventId == eventId)
            .Should()
            .Be(1);

        var secondIndexer = new CountingLogIndexer();

        using (var secondHost = LogProcessorHostFactory.Create(
            _kafka,
            _openSearch,
            groupId: groupId,
            batchSize: 1,
            flushIntervalMs: 200,
            topic: topic,
            dlqTopic: dlqTopic,
            configureServices: services =>
            {
                services.RemoveAll<ILogIndexer>();
                services.AddSingleton<ILogIndexer>(secondIndexer);
            }))
        {
            await secondHost.StartAsync();

            await Task.Delay(2_000);

            await secondHost.StopAsync();
        }

        secondIndexer.IndexedEvents
            .Any(x => x.EventId == eventId)
            .Should()
            .BeFalse();
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