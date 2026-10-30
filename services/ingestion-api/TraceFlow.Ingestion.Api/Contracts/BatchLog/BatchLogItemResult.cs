namespace TraceFlow.Ingestion.Api.Contracts.BatchLog;

public sealed record BatchLogItemResult(
    int Index,
    bool Accepted,
    Ulid? EventId,
    string? Error);
