namespace TraceFlow.Api.Infrastructure.Options;

public sealed class InternalServiceOptions
{
    public const string SectionName = "InternalService";

    public required string Secret { get; init; }
}