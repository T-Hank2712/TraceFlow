namespace TraceFlow.Api.Application.Workspaces.Commands.ArchiveWorkspace;

public record ArchiveWorkspaceResponse(
    Ulid Id,
    string Name,
    string Slug,
    string Status,
    DateTime UpdatedAt);