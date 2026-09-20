using TraceFlow.Ingestion.Api.Contracts;

namespace TraceFlow.Ingestion.Api.Ingestion;

public interface IIngestLogService
{
    Task<IngestLogResult> IngestAsync(
        IngestLogRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken);
}