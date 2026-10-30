namespace TraceFlow.Ingestion.Api.Services.Ingestion;

public interface IIngestLogService
{
    Task<Result<IngestLogResponse>> IngestAsync(
        IngestLogRequest request,
        AuthenticatedContext authentication,
        string? idempotencyKey,
        CancellationToken cancellationToken);

    Task<Result<BatchLogResponse>> BatchLogAsync(
        BatchLogRequest request,
        AuthenticatedContext authentication,
        string? idempotencyKey,
        CancellationToken cancellationToken
    );
}
