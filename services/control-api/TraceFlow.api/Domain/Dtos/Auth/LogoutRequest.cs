namespace TraceFlow.Api.Domain.Dtos.Auth;

public record LogoutRequest(
    string RefreshToken
);