namespace TraceFlow.Ingestion.Api.Configuration;

public sealed class KafkaOptions
{
    public required string BootstrapServers { get; init; }
    public required string Topic { get; init; }
    public int DeliveryTimeoutSeconds { get; init; }
}
