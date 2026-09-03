using MediatR;
using TraceFlow.api.Application.Auth.Register;

namespace TraceFlow.Api.Application.Users.Commands.CreateUser;
public record RegisterCommand(
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    string Password,
    string ConfirmPassword
) : IRequest<RegisterResponse>;