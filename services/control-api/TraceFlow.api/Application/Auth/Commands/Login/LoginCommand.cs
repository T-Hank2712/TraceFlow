using MediatR;

namespace TraceFlow.Api.Application.Auth.Login;
public record LoginCommand(
    string Identifier,
    string Password
) : IRequest<LoginResponse>;

