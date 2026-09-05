using MediatR;

namespace TraceFlow.Api.Application.Auth.Commands.Logout;

public record LogoutCommand(
    Ulid UserId,
    string RefreshToken
) : IRequest<LogoutResponse>;