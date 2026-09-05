namespace TraceFlow.Api.Application.Auth.Commands.RefreshSession;

public record RefreshSessionResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt
);