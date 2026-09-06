namespace TraceFlow.Api.Domain.Dtos.Workspaces;

public record CreateWorkspaceRequest(
    string Name,
    string Slug,
    string? Description
);