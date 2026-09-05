using MediatR;

namespace TraceFlow.Api.Application.Auth.Commands.RefreshSession;

public record RefreshSessionCommand(
    string RefreshToken
) : IRequest<RefreshSessionResponse>;