namespace TraceFlow.Api.Domain.Dtos.Workspaces;

public record UpdateWorkspaceRequest(
    string? Name,
    string? Slug,
    string? Description);