using MediatR;

namespace TraceFlow.Api.Application.ApiKeys.Commands.RevokeApiKey;

public sealed record RevokeApiKeyCommand(
    Ulid WorkspaceId,
    Ulid ProjectId,
    Ulid ApplicationId,
    Ulid ApiKeyId,
    Ulid UserId) : IRequest<RevokeApiKeyResponse>;