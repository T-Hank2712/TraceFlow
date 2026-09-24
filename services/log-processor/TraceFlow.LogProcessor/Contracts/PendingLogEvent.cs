using Confluent.Kafka;

namespace TraceFlow.LogProcessor.Contracts;

public sealed record PendingLogEvent(
    LogEvent Event,
    TopicPartitionOffset Offset);
