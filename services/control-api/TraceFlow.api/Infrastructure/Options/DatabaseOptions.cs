public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public int MaxRetryCount { get; init; }
    public int MaxRetryDelaySeconds { get; init; }
}