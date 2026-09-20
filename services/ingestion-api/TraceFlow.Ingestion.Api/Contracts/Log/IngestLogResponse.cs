namespace TraceFlow.Ingestion.Api.Contracts.Log;

public sealed record IngestLogResponse(
    Ulid EventId,
    bool Status,
    DateTimeOffset AcceptedAt);
