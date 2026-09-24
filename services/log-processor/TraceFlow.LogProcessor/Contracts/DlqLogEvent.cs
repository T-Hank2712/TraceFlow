namespace TraceFlow.LogProcessor.Contracts;

public sealed record DlqLogEvent(
    LogEvent Event,
    string FailureType,
    string FailureReason,
    DateTimeOffset FailedAt);
