namespace TraceFlow.Api.Infrastructure.Options;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public required string ConnectionString { get; init; }
    public required string ApiKeyRevocationKeyPrefix { get; init; }
}
