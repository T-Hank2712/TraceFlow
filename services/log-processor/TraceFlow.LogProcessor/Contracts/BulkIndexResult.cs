namespace TraceFlow.LogProcessor.Contracts;

public sealed record BulkIndexResult(
    IReadOnlyCollection<LogEvent> SucceededEvents,
    IReadOnlyCollection<LogEvent> FailedEvents);
