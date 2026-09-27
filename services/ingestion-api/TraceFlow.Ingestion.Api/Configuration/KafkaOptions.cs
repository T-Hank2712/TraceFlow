namespace TraceFlow.Ingestion.Api.Configuration;

public sealed class KafkaOptions
{
    public required string BootstrapServers { get; init; }
    public required string Topic { get; init; }
    public int MessageSendMaxRetries { get; init; }

    public int RetryBackoffMs { get; init; }

    public int DeliveryTimeoutMs { get; init; }

    public int MessageTimeoutMs { get; init; }

    public int QueueBufferingMaxMessages { get; init; }

    public int QueueBufferingMaxKbytes { get; init; }

    public int LingerMs { get; init; }
}
