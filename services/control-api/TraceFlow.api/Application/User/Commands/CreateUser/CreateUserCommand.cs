using MediatR;

namespace TraceFlow.Api.Application.Users.Commands.CreateUser;
public record CreateUserCommand(
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    string Password,
    string ConfirmPassword
) : IRequest<Ulid>;