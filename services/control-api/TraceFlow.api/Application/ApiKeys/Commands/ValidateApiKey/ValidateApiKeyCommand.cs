using MediatR;

namespace TraceFlow.Api.Application.ApiKeys.Commands.ValidateApiKey;

public sealed record ValidateApiKeyCommand(
    string ApiKey) : IRequest<ValidateApiKeyResponse>;