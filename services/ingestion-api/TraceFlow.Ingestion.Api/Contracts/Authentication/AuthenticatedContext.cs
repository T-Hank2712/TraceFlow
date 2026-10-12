namespace TraceFlow.Ingestion.Api.Contracts.Authentication;

public sealed record AuthenticatedContext(
    string ApiKeyFingerprint,
    ApiKeyValidationResult Tenant);
