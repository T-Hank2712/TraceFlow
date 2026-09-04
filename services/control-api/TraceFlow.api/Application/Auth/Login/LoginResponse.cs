namespace TraceFlow.Api.Application.Auth.Login;
public record LoginResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
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