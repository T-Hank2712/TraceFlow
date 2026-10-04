namespace TraceFlow.Api.Infrastructure.Options;

public sealed class ApiKeySecurityOptions
{
    public const string SectionName = "ApiKeySecurity";

    public required string Pepper { get; init; }
}