using TraceFlow.Ingestion.Api.Contracts.RateLimiting;

namespace TraceFlow.Ingestion.Api.Services.RateLimiting;

public interface IRateLimiter
{
    Task<RateLimitResult> CheckAsync(
        string apiKey,
        CancellationToken cancellationToken = default);
}