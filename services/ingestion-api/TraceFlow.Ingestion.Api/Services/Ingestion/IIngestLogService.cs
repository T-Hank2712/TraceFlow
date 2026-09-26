using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.Contracts.BatchLog;
using TraceFlow.Ingestion.Api.Contracts;
using TraceFlow.Ingestion.Api.Contracts.Authentication;

namespace TraceFlow.Ingestion.Api.Services.Ingestion;

public interface IIngestLogService
{
    Task<Result<IngestLogResponse>> IngestAsync(
        IngestLogRequest request,
        AuthenticatedContext authentication,
        CancellationToken cancellationToken);

    Task<Result<BatchLogResponse>> BatchLogAsync(
        BatchLogRequest request,
        AuthenticatedContext authentication,
        CancellationToken cancellationToken
    );
}
