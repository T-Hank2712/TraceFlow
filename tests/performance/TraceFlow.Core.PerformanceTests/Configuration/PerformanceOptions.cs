namespace TraceFlow.Core.PerformanceTests.Configuration;

public sealed class PerformanceOptions
{
    public string BaseUrl { get; init; } =
        "http://localhost:5100";

    public string ApiKey { get; init; } =
        string.Empty;

    public int Rate { get; init; } = 10;

    public int DurationSeconds { get; init; } = 30;

    public int BatchSize { get; init; } = 1;
}