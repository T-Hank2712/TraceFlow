namespace TraceFlow.LogProcessor.Configurations;

public sealed class KafkaOptions
{
    public required string BootstrapServers { get; init; }
    public required string GroupId { get; init; }
    public required string Topic { get; init; }
    public required string DlqTopic { get; init; }
    public int PollTimeoutMs { get; set; }
}