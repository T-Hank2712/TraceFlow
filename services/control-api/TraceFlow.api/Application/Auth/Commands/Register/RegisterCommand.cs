using MediatR;

namespace TraceFlow.Api.Application.Auth.Commands.Register;
public record RegisterCommand(
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    string Password,
    string ConfirmPassword
) : IRequest<RegisterResponse>;