namespace TraceFlow.Api.Application.ApiKeys.Commands.RevokeApiKey;

public sealed record RevokeApiKeyCommand(
    Ulid ApiKeyId,
    Ulid UserId) : IRequest<RevokeApiKeyResponse>;
