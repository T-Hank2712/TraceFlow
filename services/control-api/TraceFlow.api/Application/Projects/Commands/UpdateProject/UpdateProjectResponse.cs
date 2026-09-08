namespace TraceFlow.Api.Application.Projects.Commands.UpdateProject;

public record UpdateProjectResponse(
    Ulid Id,
    Ulid WorkspaceId,
    Ulid CreatedByUserId,
    string Name,
    string Slug,
    string? Description,
    string Status,
    DateTime UpdatedAt);