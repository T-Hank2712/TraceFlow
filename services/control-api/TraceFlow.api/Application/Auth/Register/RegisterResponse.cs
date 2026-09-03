namespace TraceFlow.api.Application.Auth.Register;

public record RegisterResponse(
    string Email,
    string UserName,
    string FirstName,
    string LastName
);