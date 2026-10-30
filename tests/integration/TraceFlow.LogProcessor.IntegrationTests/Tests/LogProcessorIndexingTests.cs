using FluentAssertions;
using TraceFlow.LogProcessor.Contracts;
using TraceFlow.LogProcessor.IntegrationTests.Infrastructure;

namespace TraceFlow.LogProcessor.IntegrationTests.Tests;

[Collection("Log Processor Integration")]
public sealed class LogProcessorIndexingTests
{
    private readonly KafkaFixture _kafka;
    private readonly OpenSearchFixture _openSearch;

    public LogProcessorIndexingTests(
        KafkaFixture kafka,
        OpenSearchFixture openSearch)
    {
        _kafka = kafka;
        _openSearch = openSearch;
    }

    [Fact]
    public async Task LogProcessor_WhenLogEventIsConsumed_ShouldIndexDocumentIntoOpenSearch()
    {
        var eventId = Ulid.NewUlid().ToString();

        using var host = LogProcessorHostFactory.Create(
            _kafka,
            _openSearch,
            groupId: $"traceflow-log-processor-test-{Ulid.NewUlid()}");

        await host.StartAsync();

        using var producer = new KafkaTestProducer(_kafka.BootstrapServers);

        var logEvent = new LogEvent(
            EventId: eventId,
            WorkspaceId: Ulid.NewUlid().ToString(),
            ProjectId: Ulid.NewUlid().ToString(),
            ApplicationId: Ulid.NewUlid().ToString(),
            Environment: "Production",
            Service: " checkout-api ",
            Level: 4,
            Message: " Payment failed ",
            Timestamp: DateTimeOffset.UtcNow.AddMinutes(-1),
            TraceId: "trace-log-processor-1",
            CorrelationId: "corr-log-processor-1",
            Metadata: new Dictionary<string, object?>
            {
                ["orderId"] = "order-123"
            });

        await producer.ProduceLogEventAsync(
            "traceflow.logs",
            logEvent);

        var indexedEvent = await _openSearch.WaitForLogEventAsync(
            eventId,
            TimeSpan.FromSeconds(20));

        indexedEvent.EventId.Should().Be(eventId);
        indexedEvent.WorkspaceId.Should().Be(logEvent.WorkspaceId);
        indexedEvent.ProjectId.Should().Be(logEvent.ProjectId);
        indexedEvent.ApplicationId.Should().Be(logEvent.ApplicationId);
        indexedEvent.Environment.Should().Be("production");
        indexedEvent.Service.Should().Be("checkout-api");
        indexedEvent.Message.Should().Be("Payment failed");
        indexedEvent.Level.Should().Be(4);
        indexedEvent.TraceId.Should().Be("trace-log-processor-1");
        indexedEvent.CorrelationId.Should().Be("corr-log-processor-1");

        await host.StopAsync();
    }
}