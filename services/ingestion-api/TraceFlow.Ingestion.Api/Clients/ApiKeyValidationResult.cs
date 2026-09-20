namespace TraceFlow.Ingestion.Api.Clients;

public sealed record ApiKeyValidationResult(
    bool Valid,
    Ulid? WorkspaceId,
    Ulid? ProjectId,
    Ulid? ApplicationId,
    string? Environment);