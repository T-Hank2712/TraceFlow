using FluentAssertions;
using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.IntegrationTests.Infrastructure;

namespace TraceFlow.LogProcessor.IntegrationTests.Tests;

[Collection("Log Processor Integration")]
public sealed class LogProcessorEventProcessingTests
{
    private readonly KafkaFixture _kafka;
    private readonly OpenSearchFixture _openSearch;

    public LogProcessorEventProcessingTests(
        KafkaFixture kafka,
        OpenSearchFixture openSearch)
    {
        _kafka = kafka;
        _openSearch = openSearch;
    }

    [Fact]
    public async Task LogProcessor_WhenLogEventIsConsumed_ShouldNormalizeEventBeforeIndexing()
    {
        var eventId = Ulid.NewUlid().ToString();

        using var host = LogProcessorHostFactory.Create(
            _kafka,
            _openSearch,
            groupId: $"traceflow-log-processor-normalization-test-{Guid.NewGuid()}");

        await host.StartAsync();

        try
        {
            using var producer = new KafkaTestProducer(_kafka.BootstrapServers);

            var inputTimestamp = new DateTimeOffset(
                2026,
                9,
                28,
                10,
                30,
                0,
                TimeSpan.FromHours(7));

            var logEvent = new LogEvent(
                EventId: eventId,
                WorkspaceId: Ulid.NewUlid().ToString(),
                ProjectId: Ulid.NewUlid().ToString(),
                ApplicationId: Ulid.NewUlid().ToString(),
                Environment: "Production",
                Service: " checkout-api ",
                Level: 4,
                Message: " Payment failed ",
                Timestamp: inputTimestamp,
                TraceId: "trace-normalization-1",
                CorrelationId: "corr-normalization-1",
                Metadata: new Dictionary<string, object?>
                {
                    ["orderId"] = "order-123",
                    ["attempt"] = 2,
                    ["retryable"] = true
                });

            await producer.ProduceLogEventAsync(
                "traceflow.logs",
                logEvent);

            var indexedEvent = await _openSearch.WaitForLogEventAsync(
                eventId,
                TimeSpan.FromSeconds(20));

            indexedEvent.EventId.Should().Be(eventId);
            indexedEvent.Environment.Should().Be("production");
            indexedEvent.Service.Should().Be("checkout-api");
            indexedEvent.Message.Should().Be("Payment failed");
            indexedEvent.Timestamp.Should().Be(inputTimestamp.ToUniversalTime());

            indexedEvent.Metadata.Should().NotBeNull();
            indexedEvent.Metadata!.Should().ContainKey("orderId");
            indexedEvent.Metadata.Should().ContainKey("attempt");
            indexedEvent.Metadata.Should().ContainKey("retryable");
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [Fact]
    public async Task LogProcessor_WhenBatchSizeIsReached_ShouldIndexAllEventsIntoOpenSearch()
    {
        var firstEventId = Ulid.NewUlid().ToString();
        var secondEventId = Ulid.NewUlid().ToString();

        using var host = LogProcessorHostFactory.Create(
            _kafka,
            _openSearch,
            groupId: $"traceflow-log-processor-batch-size-test-{Guid.NewGuid()}",
            batchSize: 2,
            flushIntervalMs: 10_000);

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
                Message: "First event",
                Timestamp: DateTimeOffset.UtcNow.AddMinutes(-2),
                TraceId: "trace-batch-1",
                CorrelationId: "corr-batch-1",
                Metadata: null);

            var secondEvent = new LogEvent(
                EventId: secondEventId,
                WorkspaceId: Ulid.NewUlid().ToString(),
                ProjectId: Ulid.NewUlid().ToString(),
                ApplicationId: Ulid.NewUlid().ToString(),
                Environment: "production",
                Service: "payment-api",
                Level: 4,
                Message: "Second event",
                Timestamp: DateTimeOffset.UtcNow.AddMinutes(-1),
                TraceId: "trace-batch-2",
                CorrelationId: "corr-batch-2",
                Metadata: null);

            await producer.ProduceLogEventAsync("traceflow.logs", firstEvent);
            await producer.ProduceLogEventAsync("traceflow.logs", secondEvent);

            var indexedFirst = await _openSearch.WaitForLogEventAsync(
                firstEventId,
                TimeSpan.FromSeconds(20));

            var indexedSecond = await _openSearch.WaitForLogEventAsync(
                secondEventId,
                TimeSpan.FromSeconds(20));

            indexedFirst.EventId.Should().Be(firstEventId);
            indexedFirst.Service.Should().Be("checkout-api");
            indexedFirst.Message.Should().Be("First event");

            indexedSecond.EventId.Should().Be(secondEventId);
            indexedSecond.Service.Should().Be("payment-api");
            indexedSecond.Message.Should().Be("Second event");
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [Fact]
    public async Task LogProcessor_WhenFlushIntervalElapses_ShouldIndexBufferedEvent()
    {
        var eventId = Ulid.NewUlid().ToString();

        using var host = LogProcessorHostFactory.Create(
            _kafka,
            _openSearch,
            groupId: $"traceflow-log-processor-flush-interval-test-{Guid.NewGuid()}",
            batchSize: 10,
            flushIntervalMs: 200);

        await host.StartAsync();

        try
        {
            using var producer = new KafkaTestProducer(_kafka.BootstrapServers);

            var logEvent = new LogEvent(
                EventId: eventId,
                WorkspaceId: Ulid.NewUlid().ToString(),
                ProjectId: Ulid.NewUlid().ToString(),
                ApplicationId: Ulid.NewUlid().ToString(),
                Environment: "production",
                Service: "inventory-api",
                Level: 2,
                Message: "Buffered event",
                Timestamp: DateTimeOffset.UtcNow,
                TraceId: "trace-flush-interval-1",
                CorrelationId: "corr-flush-interval-1",
                Metadata: null);

            await producer.ProduceLogEventAsync(
                "traceflow.logs",
                logEvent);

            var indexedEvent = await _openSearch.WaitForLogEventAsync(
                eventId,
                TimeSpan.FromSeconds(20));

            indexedEvent.EventId.Should().Be(eventId);
            indexedEvent.Service.Should().Be("inventory-api");
            indexedEvent.Message.Should().Be("Buffered event");
        }
        finally
        {
            await host.StopAsync();
        }
    }
}