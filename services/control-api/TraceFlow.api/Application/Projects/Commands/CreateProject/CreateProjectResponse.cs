namespace TraceFlow.Api.Application.Projects.Commands.CreateProject;

public record CreateProjectResponse(
    Ulid Id,
    Ulid WorkspaceId,
    Ulid CreatedByUserId,
    string Name,
    string Slug,
    string? Description,
    string Status,
    DateTime CreatedAt
    );