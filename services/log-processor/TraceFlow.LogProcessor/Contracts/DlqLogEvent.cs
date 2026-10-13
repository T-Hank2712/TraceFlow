namespace TraceFlow.LogProcessor.Contracts;

public sealed record DlqLogEvent(
    LogEvent? Event,
    string? RawPayload,
    string FailureType,
    string FailureReason,
    string Topic,
    int Partition,
    long Offset,
    DateTimeOffset FailedAt);
