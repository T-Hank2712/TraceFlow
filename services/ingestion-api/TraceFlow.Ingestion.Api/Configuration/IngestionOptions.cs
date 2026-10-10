namespace TraceFlow.Ingestion.Api.Configuration;

public sealed class IngestionOptions
{
    public int MaxMessageLength { get; init; }
    public int MaxServiceLength { get; init; }
    public int MaxBatchSize { get; init; }
    public long MaxRequestBodyBytes { get; init; }
    public int MaxTraceIdLength { get; init; }
    public int MaxCorrelationIdLength { get; init; }
    public int MaxMetadataKeys { get; init; }
    public int MaxMetadataDepth { get; init; }
    public int MaxMetadataKeyLength { get; init; }
    public int MaxMetadataStringValueLength { get; init; }
    public int MaxMetadataArrayLength { get; init; }
}
