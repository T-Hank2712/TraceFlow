namespace TraceFlow.Ingestion.Api.Contracts;

public sealed record ErrorResponse(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Details = null);
