namespace TraceFlow.Api.Domain.Constants;

public static class InternalServiceAuthentication
{
    public const string Scheme = "InternalService";
    public const string Policy = "InternalService";

    public const string SecretHeader = "X-Internal-Secret";

    public const string ServiceClaimType = "traceflow:service";
    public const string ServiceName = "ingestion-api";
}