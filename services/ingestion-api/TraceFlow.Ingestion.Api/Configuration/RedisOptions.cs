namespace TraceFlow.Ingestion.Api.Configurations;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public required string ConnectionString { get; init; }
}