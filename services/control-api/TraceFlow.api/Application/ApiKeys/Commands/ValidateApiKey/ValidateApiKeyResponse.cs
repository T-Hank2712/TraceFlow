namespace TraceFlow.Api.Application.ApiKeys.Commands.ValidateApiKey;

public sealed record ValidateApiKeyResponse(
    bool Valid,
    Ulid? WorkspaceId,
    Ulid? ProjectId,
    Ulid? ApplicationId,
    string? Environment);