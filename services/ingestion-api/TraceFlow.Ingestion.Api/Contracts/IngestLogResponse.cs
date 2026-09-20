namespace TraceFlow.Ingestion.Api.Contracts;

public sealed record IngestLogResponse(
    Ulid EventId,
    string Status,
    DateTimeOffset AcceptedAt);