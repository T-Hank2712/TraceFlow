namespace TraceFlow.LogProcessor.Contracts;

public sealed record BatchProcessResult(
    IReadOnlyCollection<TopicPartitionOffset> ProcessedOffsets);
