namespace TraceFlow.Ingestion.Api.Services.RateLimiting;

public interface IRateLimiter
{
    Task<RateLimitResult> CheckAsync(
        string apiKeyFingerprint,
        CancellationToken cancellationToken = default);
}
