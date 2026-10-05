namespace TraceFlow.Api.Infrastructure.Options;

public sealed class TrustedProxyOptions
{
    public const string SectionName = "TrustedProxies";
    public string[] KnownNetworks { get; init; } = [];
}