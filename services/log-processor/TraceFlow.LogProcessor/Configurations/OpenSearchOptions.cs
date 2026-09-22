namespace TraceFlow.LogProcessor.Configurations;

public sealed class OpenSearchOptions
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string Index { get; init; }
    public bool SkipTlsVerify { get; set; }
}
