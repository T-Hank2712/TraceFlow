namespace TraceFlow.Api.Application.Auth.Queries.GetCurrentUser;

public record CurrentUserResponse(
    Ulid Id,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    string Role,
    string Status
);