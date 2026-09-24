using Confluent.Kafka;

namespace TraceFlow.LogProcessor.Contracts;

public sealed record BatchProcessResult(
    IReadOnlyCollection<TopicPartitionOffset> ProcessedOffsets);
