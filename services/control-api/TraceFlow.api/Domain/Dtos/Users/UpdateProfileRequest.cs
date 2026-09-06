namespace TraceFlow.Api.Domain.Dtos.Users;

public record UpdateProfileRequest(
    string? UserName,
    string? FirstName,
    string? LastName
);