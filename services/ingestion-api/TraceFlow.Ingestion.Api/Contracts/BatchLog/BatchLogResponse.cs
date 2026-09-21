namespace TraceFlow.Ingestion.Api.Contracts.BatchLog;

public sealed record BatchLogResponse(
    Ulid BatchId,
    int Total,
    int Accepted,
    int Rejected,
    IReadOnlyList<BatchLogItemResult> Results);
