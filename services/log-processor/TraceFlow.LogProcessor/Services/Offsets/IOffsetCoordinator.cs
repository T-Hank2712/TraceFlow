using Confluent.Kafka;

namespace TraceFlow.LogProcessor.Services.Offsets;

public interface IOffsetCoordinator
{
    void MarkProcessed(
        IReadOnlyCollection<TopicPartitionOffset> offsets);

    IReadOnlyCollection<TopicPartitionOffset>
        GetCommittableOffsets();
}