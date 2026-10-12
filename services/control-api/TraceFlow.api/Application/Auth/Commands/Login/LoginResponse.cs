namespace TraceFlow.Api.Application.Auth.Commands.Login;

public record LoginResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    LoginUserResponse User
);

public record LoginUserResponse(
    Ulid Id,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    string Role
);
