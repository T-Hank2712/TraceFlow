namespace TraceFlow.LogProcessor.Configurations;

public sealed class ProcessorOptions
{
    public int MaxRetries { get; set; }
    public int RetryBackoffMs { get; set; }
    public int BatchSize { get; set; }
    public int FlushIntervalMs { get; set; }
    public int QueueCapacity { get; set; }
    public int ShutdownFlushTimeoutSeconds { get; set; }
}
