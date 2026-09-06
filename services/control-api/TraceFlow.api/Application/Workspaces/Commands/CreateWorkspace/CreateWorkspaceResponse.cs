namespace TraceFlow.Api.Application.Workspaces.Commands.CreateWorkspace;

public record CreateWorkspaceResponse(
    Ulid Id,
    string Name,
    string Slug,
    string? Description,
    string Status,
    string Role
);