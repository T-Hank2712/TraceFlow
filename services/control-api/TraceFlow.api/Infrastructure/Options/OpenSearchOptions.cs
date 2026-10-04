namespace TraceFlow.Api.Infrastructure.Options;

public sealed class OpenSearchOptions
{
    public const string SectionName = "OpenSearch";

    public required string Url { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string Index { get; init; }

    public bool SkipTlsVerify { get; init; }
}