namespace TraceFlow.Ingestion.Api.Configuration;

public sealed class ControlApiOptions
{
    public required string BaseUrl { get; init; }
    public required string ValidateApiPath { get; init; }
    public required string InternalServiceSecret { get; init; }
    public required int TimeoutSeconds { get; init; }
}
