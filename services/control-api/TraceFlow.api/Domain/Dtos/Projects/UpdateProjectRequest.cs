namespace TraceFlow.Api.Domain.Dtos.Projects;

public record UpdateProjectRequest(
    string? Name,
    string? Slug,
    string? Description);