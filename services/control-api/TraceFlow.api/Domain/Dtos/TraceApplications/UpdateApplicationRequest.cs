namespace TraceFlow.Api.Domain.Dtos.TraceApplications;

public record UpdateApplicationRequest(
    string? Name,
    string? Slug,
    string? Description);