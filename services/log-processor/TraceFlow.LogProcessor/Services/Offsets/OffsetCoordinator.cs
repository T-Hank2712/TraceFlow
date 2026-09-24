using Confluent.Kafka;

namespace TraceFlow.LogProcessor.Services.Offsets;

public sealed class OffsetCoordinator : IOffsetCoordinator
{
    private sealed class PartitionState
    {
        public long NextOffset { get; set; }
        public long LastCommittedOffset { get; set; } = -1;

        public SortedSet<long> ProcessedOffsets { get; } = [];
    }

    private readonly object _lock = new();

    private readonly Dictionary<
        TopicPartition,
        PartitionState> _partitions = [];

    public void MarkProcessed(
        IReadOnlyCollection<TopicPartitionOffset> offsets)
    {
        if (offsets.Count == 0)
        {
            return;
        }

        lock (_lock)
        {
            foreach (var offset in offsets)
            {
                if (!_partitions.TryGetValue(
                        offset.TopicPartition,
                        out var state))
                {
                    state = new PartitionState
                    {
                        NextOffset = offset.Offset
                    };

                    _partitions.Add(
                        offset.TopicPartition,
                        state);
                }

                state.ProcessedOffsets.Add(
                    offset.Offset);
            }
        }
    }

    public IReadOnlyCollection<TopicPartitionOffset>
        GetCommittableOffsets()
    {
        lock (_lock)
        {
            var result = new List<TopicPartitionOffset>();

            foreach (var pair in _partitions)
            {
                var topicPartition = pair.Key;
                var state = pair.Value;

                while (state.ProcessedOffsets.Remove(
                    state.NextOffset))
                {
                    state.NextOffset++;
                }

                var commitOffset = state.NextOffset;

                if (commitOffset <= state.LastCommittedOffset)
                {
                    continue;
                }

                result.Add(
                    new TopicPartitionOffset(
                        topicPartition,
                        new Offset(commitOffset)));

                state.LastCommittedOffset = commitOffset;
            }

            return result;
        }
    }
}