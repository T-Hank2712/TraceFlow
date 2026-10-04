namespace TraceFlow.Api.Infrastructure.Options;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    public required RateLimitPolicyOptions Global { get; init; }
    public required RateLimitPolicyOptions Auth { get; init; }
    public required RateLimitPolicyOptions Internal { get; init; }
    public required RateLimitPolicyOptions Search { get; init; }
    public required RateLimitPolicyOptions Health { get; init; }
}

public sealed class RateLimitPolicyOptions
{
    public int PermitLimit { get; init; }
    public int WindowSeconds { get; init; }
    public int QueueLimit { get; init; }
}
