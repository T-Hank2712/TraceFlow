using TraceFlow.Ingestion.Api.Contracts.Log;

namespace TraceFlow.Ingestion.Api.Contracts.BatchLog;

public sealed record BatchLogResult(
    Ulid BatchId,
    IReadOnlyList<EnrichedLogEvent> Events);
