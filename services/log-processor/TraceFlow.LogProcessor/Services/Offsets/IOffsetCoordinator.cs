namespace TraceFlow.LogProcessor.Services.Offsets;

public interface IOffsetCoordinator
{
    void MarkProcessed(
        IReadOnlyCollection<TopicPartitionOffset> offsets);

    IReadOnlyCollection<TopicPartitionOffset>
        GetCommittableOffsets();

    void RemovePartitions(
        IReadOnlyCollection<TopicPartition> partitions);
}
