using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.IntegrationTests.Infrastructure;
using TraceFlow.LogProcessor.Services.OpenSearch;

namespace TraceFlow.LogProcessor.IntegrationTests.Tests;

[Collection("Log Processor Integration")]
public sealed class LogProcessorDlqTests
{
    private readonly KafkaFixture _kafka;
    private readonly OpenSearchFixture _openSearch;

    public LogProcessorDlqTests(
        KafkaFixture kafka,
        OpenSearchFixture openSearch)
    {
        _kafka = kafka;
        _openSearch = openSearch;
    }

    [Fact]
    public async Task LogProcessor_WhenBulkIndexPartiallyFails_ShouldPublishFailedEventToDlqTopic()
    {
        var firstEventId = Ulid.NewUlid().ToString();
        var secondEventId = Ulid.NewUlid().ToString();

        var topic = $"traceflow.logs.dlq-test-{Guid.NewGuid()}";
        var dlqTopic = $"{topic}.dlq";

        await _kafka.CreateTopicAsync(topic);
        await _kafka.CreateTopicAsync(dlqTopic);

        var fakeIndexer = new PartialFailingLogIndexer();

        using var dlqConsumer = new KafkaTestConsumer<DlqLogEvent>(
            _kafka.BootstrapServers,
            dlqTopic);

        using var host = LogProcessorHostFactory.Create(
            _kafka,
            _openSearch,
            groupId: $"traceflow-log-processor-dlq-test-{Guid.NewGuid()}",
            batchSize: 2,
            flushIntervalMs: 10_000,
            topic: topic,
            dlqTopic: dlqTopic,
            configureServices: services =>
            {
                services.RemoveAll<ILogIndexer>();
                services.AddSingleton<ILogIndexer>(fakeIndexer);
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
                TraceId: "trace-dlq-1",
                CorrelationId: "corr-dlq-1",
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
                TraceId: "trace-dlq-2",
                CorrelationId: "corr-dlq-2",
                Metadata: null);
                
            await producer.ProduceLogEventAsync(topic, firstEvent);
            await producer.ProduceLogEventAsync(topic, secondEvent);

            var dlqEvent = dlqConsumer.ConsumeUntil(
                value => value.Event.EventId == secondEventId,
                TimeSpan.FromSeconds(20));

            dlqEvent.Event.EventId.Should().Be(secondEventId);
            dlqEvent.Event.Service.Should().Be("payment-api");
            dlqEvent.FailureType.Should().Be("OpenSearchItemFailure");
            dlqEvent.FailureReason.Should().Be("OpenSearch failed to index the event.");
            dlqEvent.FailedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(20));
        }
        finally
        {
            await host.StopAsync();
        }
    }
}