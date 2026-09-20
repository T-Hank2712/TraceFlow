using TraceFlow.Ingestion.Api.Contracts.Log;

namespace TraceFlow.Ingestion.Api.Contracts.BatchLog;

public sealed record BatchLogRequest(
    IReadOnlyList<IngestLogRequest> Logs
);
