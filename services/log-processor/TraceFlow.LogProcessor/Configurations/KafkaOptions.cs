namespace TraceFlow.LogProcessor.Configurations;

public sealed class KafkaOptions
{
    public required string BootstrapServers { get; init; }
    public required string GroupId { get; init; }
    public required string Topic { get; init; }
    public required string DlqTopic { get; init; }
    public int PollTimeoutMs { get; set; }
    public int ProducerFlushTimeoutSeconds { get; set; }
    public int DlqPublishConcurrency { get; set; }
    public int QueuedMinMessages { get; set; }
    public int QueuedMaxMessagesKbytes { get; set; }
    public int FetchMinBytes { get; set; }
    public int FetchWaitMaxMs { get; set; }
    public int MaxPartitionFetchBytes { get; set; }
}
