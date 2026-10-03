namespace TraceFlow.Api.Application.Workspaces.Commands.UpdateWorkspace;

public record UpdateWorkspaceResponse(
    Ulid WorkspaceId,
    string Name,
    string Slug,
    string? Description,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
