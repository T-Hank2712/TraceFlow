using MediatR;

namespace TraceFlow.Api.Application.Auth.Commands.Login;
public record LoginCommand(
    string Identifier,
    string Password
) : IRequest<LoginResponse>;

