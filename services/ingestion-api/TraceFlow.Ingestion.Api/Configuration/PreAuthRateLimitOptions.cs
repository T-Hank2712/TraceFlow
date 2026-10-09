namespace TraceFlow.Ingestion.Api.Configuration;

public sealed class PreAuthRateLimitOptions
{
    public const string SectionName = "PreAuthRateLimiting";

    public int PermitLimit { get; init; }

    public int WindowSeconds { get; init; }

    public int QueueLimit { get; init; }
}