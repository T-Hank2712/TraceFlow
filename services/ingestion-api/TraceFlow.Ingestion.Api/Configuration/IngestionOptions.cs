namespace TraceFlow.Ingestion.Api.Configuration;

public sealed class IngestionOptions
{
    public int MaxMessageLength { get; init; }
    public int MaxServiceLength { get; init; }
    public int MaxBatchSize { get; init; }
    public long MaxRequestBodyBytes { get; init; }
}
