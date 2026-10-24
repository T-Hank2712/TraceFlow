namespace TraceFlow.Api.Application.ApiKeys.Commands.CreateApiKey;

public sealed record CreateApiKeyCommand(
    Ulid ApplicationId,
    Ulid UserId,
    string Name,
    string Environment,
    int ExpirationPolicy) : IRequest<CreateApiKeyResponse>;
