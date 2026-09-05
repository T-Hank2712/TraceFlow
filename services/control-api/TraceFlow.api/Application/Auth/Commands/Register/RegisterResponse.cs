namespace TraceFlow.Api.Application.Auth.Commands.Register;

public record RegisterResponse(
    string Email,
    string UserName,
    string FirstName,
    string LastName
);