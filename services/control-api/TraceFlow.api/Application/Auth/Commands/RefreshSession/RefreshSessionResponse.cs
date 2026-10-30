namespace TraceFlow.Api.Application.Auth.Commands.RefreshSession;

public record RefreshSessionResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt
);
