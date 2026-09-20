namespace TraceFlow.Ingestion.Api.Contracts.BatchLog;

public sealed record BatchLogResponse(
    Ulid BatchId,
    int TotalProcessed,
    DateTimeOffset AcceptedAt);
