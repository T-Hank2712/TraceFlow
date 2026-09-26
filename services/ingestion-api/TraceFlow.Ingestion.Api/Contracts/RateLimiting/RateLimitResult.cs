namespace TraceFlow.Ingestion.Api.Contracts.RateLimiting;

public sealed record RateLimitResult(
    bool Allowed,
    int Limit,
    int Remaining,
    int RetryAfterSeconds);