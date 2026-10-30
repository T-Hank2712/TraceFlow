namespace TraceFlow.Ingestion.Api.Configuration;

public sealed class TrustedProxyOptions
{
    public const string SectionName = "TrustedProxies";

    public string[] KnownNetworks { get; init; } = [];
}
