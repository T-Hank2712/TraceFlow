namespace TraceFlow.Ingestion.Api.Contracts.Authentication;

public sealed record ApiKeyValidationResult(
    bool Valid,
    Ulid? WorkspaceId,
    Ulid? ProjectId,
    Ulid? ApplicationId,
    string? Environment);
