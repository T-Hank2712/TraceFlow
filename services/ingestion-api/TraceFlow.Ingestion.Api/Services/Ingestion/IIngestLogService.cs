using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.Contracts.BatchLog;
using TraceFlow.Ingestion.Api.Contracts;

namespace TraceFlow.Ingestion.Api.Services.Ingestion;

public interface IIngestLogService
{
    Task<Result<IngestLogResponse>> IngestAsync(
        IngestLogRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken);

    Task<Result<BatchLogResponse>> BatchLogAsync(
        BatchLogRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken
    );
}
