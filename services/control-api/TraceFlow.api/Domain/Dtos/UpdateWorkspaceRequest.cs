namespace TraceFlow.Api.Domain.Dtos;

public record UpdateWorkspaceRequest(
    string? Name,
    string? Slug,
    string? Description);