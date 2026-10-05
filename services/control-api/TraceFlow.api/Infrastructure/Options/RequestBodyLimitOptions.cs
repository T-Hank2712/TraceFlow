namespace TraceFlow.Api.Infrastructure.Options;

public sealed class RequestBodyLimitOptions
{
    public const string SectionName = "RequestBodyLimits";

    public long DefaultBytes { get; init; }
    public long AuthBytes { get; init; }
    public long InternalBytes { get; init; }
    public long SearchBytes { get; init; }
}