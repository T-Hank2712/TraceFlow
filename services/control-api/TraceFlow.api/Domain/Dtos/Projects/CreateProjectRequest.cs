namespace TraceFlow.Api.Domain.Dtos.Projects;

public record CreateProjectRequest(
    string Name,
    string Slug,
    string? Description);