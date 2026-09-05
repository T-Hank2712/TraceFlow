namespace TraceFlow.Api.Domain.Dtos;

public record UpdateProfileRequest(
    string? UserName,
    string? FirstName,
    string? LastName
);